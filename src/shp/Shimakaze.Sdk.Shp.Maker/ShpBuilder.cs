using System.Diagnostics;

using Shimakaze.Sdk.Pal;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shimakaze.Sdk.Shp.Maker;

internal sealed class ShpBuilder(Palette palette, int endIndex)
{
    private readonly List<string> _objects = [];
    private readonly List<string> _shadows = [];
    private readonly List<string> _houses = [];
    private readonly Dictionary<string, SequenceData> _sequenceData = [];

    private string _workFolder = Environment.CurrentDirectory;
    private string _currentName = "";
    private int _currentIndex;
    private string? _iniName;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public async IAsyncEnumerable<ShapeImageFrame> BuildAsync()
    {
        if (_objects.Count != _shadows.Count)
            throw new InvalidDataException("对象帧数列和影子帧数列数量不一致");
        if (_houses.Count is not 0 && _objects.Count != _houses.Count)
            throw new InvalidDataException("启用了所属色帧数列，但对象帧数列和所属色帧数列数量不一致");

        for (int i = 0; i < _objects.Count; i++)
        {
            string? obj = _objects[i];
            using Image<Rgba32> img = await Image.LoadAsync<Rgba32>(obj);
            Width = Math.Max(Width, img.Width);
            Height = Math.Max(Height, img.Height);

            ShapeImageFrame frame;
            if (_houses.Count is 0)
            {
                frame = Quantization(img, palette);
            }
            else
            {
                string? house = _houses[i];
                using Image<Rgba32> col = await Image.LoadAsync<Rgba32>(house);
                frame = Quantization(img, col, palette);
            }

            yield return frame.TrimAndCompress();
        }

        foreach (var shadow in _shadows)
        {
            using Image<Rgba32> sha = await Image.LoadAsync<Rgba32>(shadow);
            yield return Shadow(sha).TrimAndCompress();
        }
    }

    public async Task WriteIniAsync(TextWriter writer)
    {
        await writer.WriteLineAsync($"; Shimakaze.Sdk.Shp.Maker");
        await writer.WriteLineAsync($"[{_iniName}]");

        foreach (var item in _sequenceData)
        {
            if (item.Value.HasAngle)
            {
                int count = (item.Value.End + 1) / item.Value.AngleCount;

                await writer.WriteLineAsync($"{item.Key}={item.Value.Start},{count},{count}");
            }
            else
            {
                int count = item.Value.End;

                await writer.WriteLineAsync($"{item.Key}={item.Value.Start},{count},0,{item.Value.Angle}");
            }

        }

    }

    public void Load(string path)
    {
        using var reader = File.OpenText(path);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!line.StartsWith('#'))
            {
                if (line.StartsWith("object ", StringComparison.Ordinal))
                {
                    var data = line.Split(' ', 4, StringSplitOptions.TrimEntries);
                    Debug.Assert(data.Length is 4);
                    var relativePath = data[3];
                    var absolutePath = Path.Combine(_workFolder, relativePath);
                    absolutePath = Path.GetFullPath(absolutePath);
                    _objects.Add(absolutePath);

                    if (_currentName != data[1])
                    {
                        _currentName = data[1];
                        _sequenceData[_currentName] = new(_currentIndex, 0, data[2]);
                    }
                    else
                    {
                        _sequenceData[_currentName].End++;
                    }
                    _currentIndex++;
                }
                else
                {
                    var data = line.Split(' ', 2, StringSplitOptions.TrimEntries);
                    Debug.Assert(data.Length is 2);
                    var relativePath = data[1];
                    var absolutePath = Path.Combine(_workFolder, relativePath);
                    absolutePath = Path.GetFullPath(absolutePath);
                    switch (data[0])
                    {
                        case "shadow":
                            _shadows.Add(absolutePath);
                            break;
                        case "house":
                            _houses.Add(absolutePath);
                            break;
                    }
                }
            }
            if (line.StartsWith("#pragma sequenceName ", StringComparison.Ordinal))
            {
                _iniName = line["#pragma sequenceName ".Length..].Trim();
            }

            if (line.StartsWith("#pragma base ", StringComparison.Ordinal))
            {
                var dir = line["#pragma base ".Length..].Trim();
                _workFolder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, dir));
                continue;
            }

            if (line.StartsWith("#include ", StringComparison.Ordinal))
            {
                var file = line["#include ".Length..].Trim().Trim('"');

                file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, file));
                Load(file);
                continue;
            }
        }
    }

    /// <summary>
    /// 量化颜色
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="palette"></param>
    /// <returns></returns>
    private ShapeImageFrame Quantization(Image<Rgba32> obj, Palette palette)
    {
        using MemoryStream output = new();

        var ob = obj.Frames[0].PixelBuffer;
        for (int y = 0; y < obj.Height; y++)
        {
            Span<Rgba32> or = ob.DangerousGetRowSpan(y);
            for (int x = 0; x < obj.Width; x++)
            {
                Rgba32 op = or[x];
                byte index = GetIndex(palette, op, true);
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

    /// <summary>
    /// 量化颜色（单独量化所属色）
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="col"></param>
    /// <param name="palette"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    private ShapeImageFrame Quantization(Image<Rgba32> obj, Image<Rgba32> col, Palette palette)
    {
        using MemoryStream output = new();

        if (obj.Size != col.Size)
        {
            throw new FormatException();
        }

        SixLabors.ImageSharp.Memory.Buffer2D<Rgba32> ob = obj.Frames[0].PixelBuffer;
        SixLabors.ImageSharp.Memory.Buffer2D<Rgba32> cb = col.Frames[0].PixelBuffer;
        for (int y = 0; y < obj.Height; y++)
        {
            Span<Rgba32> or = ob.DangerousGetRowSpan(y);
            Span<Rgba32> cr = cb.DangerousGetRowSpan(y);
            for (int x = 0; x < obj.Width; x++)
            {
                Rgba32 op = or[x];
                Rgba32 cp = cr[x];
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

    /// <summary>
    /// 获取最接近的颜色索引
    /// </summary>
    /// <param name="palette"></param>
    /// <param name="pixel"></param>
    /// <param name="useHouse"></param>
    /// <returns></returns>
    private byte GetIndex(in Palette palette, in Rgba32 pixel, bool useHouse = false)
    {
        if (pixel.A is 0)
            return 0;

        double cdistance = double.MaxValue;
        byte index = 0;

        for (byte i = useHouse ? (byte)16 : (byte)32; i < endIndex; i++)
        {
            PaletteColor color = palette[i];

            double distance = Math.Sqrt(Math.Pow(color.Red - pixel.R, 2) + Math.Pow(color.Green - pixel.G, 2) + Math.Pow(color.Blue - pixel.B, 2));
            if (distance < cdistance)
            {
                index = i;
                cdistance = distance;
            }
        }

        return index;
    }

    /// <summary>
    /// 获取最接近的颜色索引
    /// </summary>
    /// <param name="palette"></param>
    /// <param name="pixel"></param>
    /// <returns></returns>
    private static byte GetHouseIndex(in Palette palette, in Rgba32 pixel)
    {
        if (pixel.A is 0)
            return 0;

        double cdistance = double.MaxValue;
        byte index = 0;
        for (byte i = 16; i < 32; i++)
        {
            PaletteColor color = palette[i];

            double distance = Math.Sqrt(Math.Pow(color.Red - pixel.R, 2) + Math.Pow(color.Green - pixel.G, 2) + Math.Pow(color.Blue - pixel.B, 2));
            if (distance < cdistance)
            {
                index = i;
                cdistance = distance;
            }
        }

        return index;
    }

    /// <summary>
    /// 生成阴影帧
    /// </summary>
    /// <param name="sha"></param>
    /// <returns></returns>
    private static ShapeImageFrame Shadow(Image<Rgba32> sha)
    {
        using MemoryStream output = new();

        SixLabors.ImageSharp.Memory.Buffer2D<Rgba32> buffer = sha.Frames[0].PixelBuffer;
        for (int y = 0; y < sha.Height; y++)
        {
            Span<Rgba32> raw = buffer.DangerousGetRowSpan(y);
            for (int x = 0; x < sha.Width; x++)
            {
                Rgba32 pixel = raw[x];
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
