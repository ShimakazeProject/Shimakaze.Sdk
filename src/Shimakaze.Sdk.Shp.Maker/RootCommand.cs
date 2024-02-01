using DotMake.CommandLine;

using Sharprompt;

using Shimakaze.Sdk.Pal;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shimakaze.Sdk.Shp.Maker;

[CliCommand(Description = "Shimakaze.Sdk Shp 生成器")]
internal sealed class RootCommand
{
    [CliOption(Description = "参考的调色板文件")]
    public FileInfo? Palette { get; set; }

    [CliOption(Description = "对象帧数列")]
    public DirectoryInfo? Objects { get; set; }

    [CliOption(Description = "影子帧数列")]
    public DirectoryInfo? Shadows { get; set; }

    [CliOption(Description = "对象的所属方着色帧数列")]
    public DirectoryInfo? Colors { get; set; }

    [CliArgument(Description = "输出的SHP(TS)文件")]
    public FileInfo? Output { get; set; }

    public void UsePrompt()
    {
        while (Palette is null)
            Palette = Prompt.Input<FileInfo>("您要使用哪一个调色板文件？");
        while (Objects is null)
            Objects = Prompt.Input<DirectoryInfo>("您的对象帧数列存放在哪里？");
        while (Shadows is null)
            Shadows = Prompt.Input<DirectoryInfo>("您的影子帧数列存放在哪里？");
        while (Colors is null)
            Colors = Prompt.Input<DirectoryInfo>("您的对象的所属方着色帧数列存放在哪里？");
        while (Output is null)
            Output = Prompt.Input<FileInfo>("您要将文件保存到哪里？");
    }

    public async Task RunAsync()
    {
        Palette palette;
        await using (var fs = Palette!.OpenRead())
            palette = PaletteReader.Read(fs);

        int width = 0;
        int height = 0;
        List<ShapeImageFrame> frames = [];

        foreach (var file in Objects!.GetFiles())
        {
            string colPath = Path.Combine(Colors!.FullName, file.Name);
            using var obj = await Image.LoadAsync<Rgba32>(file.FullName);
            using var col = await Image.LoadAsync<Rgba32>(colPath);
            width = obj.Width;
            height = obj.Height;
            frames.Add(Quantization(obj, col, palette).TrimAndCompress());
        }

        foreach (var file in Shadows!.GetFiles())
        {
            using var sha = await Image.LoadAsync<Rgba32>(file.FullName);
            frames.Add(Shadow(sha).TrimAndCompress());
        }

        await using (var fs = Output!.Create())
            ShapeWriter.Write(fs, new(new()
            {
                Width = (ushort)width,
                Height = (ushort)height,
                NumImages = (ushort)frames.Count,
            }, [.. frames]));
    }

    private static ShapeImageFrame Quantization(Image<Rgba32> obj, Image<Rgba32> col, Palette palette)
    {
        using MemoryStream output = new();

        if (obj.Size != col.Size)
            throw new FormatException();

        var ob = obj.Frames[0].PixelBuffer;
        var cb = col.Frames[0].PixelBuffer;
        for (int y = 0; y < obj.Height; y++)
        {
            Span<Rgba32> or = ob.DangerousGetRowSpan(y);
            Span<Rgba32> cr = cb.DangerousGetRowSpan(y);
            for (int x = 0; x < obj.Width; x++)
            {
                var op = or[x];
                var cp = cr[x];
                byte index = cp.A is not 0 ? GetHouseIndex(palette, cp) : GetIndex(palette, op);
                output.WriteByte(index);
            }
        }

        output.Seek(0, SeekOrigin.Begin);

        return new(new()
        {
            X = 0,
            Y = 0,
            Width = (ushort)obj.Width,
            Height = (ushort)obj.Height,
            CompressionType = ShapeFrameCompressionType.None
        }, output.ToArray());
    }

    private static byte GetIndex(in Palette palette, in Rgba32 pixel)
    {
        if (pixel.A is 0)
            return 0;

        double cdistance = double.MaxValue;
        byte index = 0;
        for (byte i = 32; i < 240; i++)
        {
            var color = palette[i];

            double distance = Math.Sqrt(Math.Pow(color.Red - pixel.R, 2) + Math.Pow(color.Green - pixel.G, 2) + Math.Pow(color.Blue - pixel.B, 2));
            if (distance < cdistance)
            {
                index = i;
                cdistance = distance;
            }
        }

        return index;
    }
    private static byte GetHouseIndex(in Palette palette, in Rgba32 pixel)
    {
        if (pixel.A is 0)
            return 0;

        double cdistance = double.MaxValue;
        byte index = 0;
        for (byte i = 16; i < 32; i++)
        {
            var color = palette[i];

            double distance = Math.Sqrt(Math.Pow(color.Red - pixel.R, 2) + Math.Pow(color.Green - pixel.G, 2) + Math.Pow(color.Blue - pixel.B, 2));
            if (distance < cdistance)
            {
                index = i;
                cdistance = distance;
            }
        }

        return index;
    }

    private static ShapeImageFrame Shadow(Image<Rgba32> sha)
    {
        using MemoryStream output = new();

        var buffer = sha.Frames[0].PixelBuffer;
        for (int y = 0; y < sha.Height; y++)
        {
            Span<Rgba32> raw = buffer.DangerousGetRowSpan(y);
            for (int x = 0; x < sha.Width; x++)
            {
                var pixel = raw[x];
                output.WriteByte(pixel.A is 0 ? (byte)0 : (byte)1);
            }
        }

        output.Seek(0, SeekOrigin.Begin);

        return new(new()
        {
            X = 0,
            Y = 0,
            Width = (ushort)sha.Width,
            Height = (ushort)sha.Height,
            CompressionType = ShapeFrameCompressionType.None
        }, output.ToArray());
    }
}
