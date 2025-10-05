using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using SkiaSharp;

namespace Shimakaze.Sdk.Engine.Shp;

internal static class ShpMaker
{
    public static void Build(IEnumerable<ShpFrameSource> frames, Stream output, Palette palette, int paletteStartIndex, int paletteEndIndex)
    {
        var shpFrames = BuildFrames(frames, palette, paletteStartIndex, paletteEndIndex).ToList();

        var width = shpFrames.Select(i => i.Width).Max();
        var height = shpFrames.Select(i => i.Height).Max();

        new ShapeImage(new()
        {
            Width = (ushort)width,
            Height = (ushort)height,
            NumImages = (ushort)shpFrames.Count,
        }, shpFrames).WriteTo(output);

    }

    private static IEnumerable<ShapeImageFrame> BuildFrames(IEnumerable<ShpFrameSource> frames, Palette palette, int paletteStartIndex, int paletteEndIndex)
    {
        foreach (var src in frames)
        {
            using var obj = SKBitmap.Decode(src.Object.FullName);

            ShapeImageFrame frame;
            if (src.House is { Exists: true })
            {
                using var col = SKBitmap.Decode(src.House.FullName);
                frame = Quantization(obj, col, palette, paletteStartIndex, paletteEndIndex);
            }
            else
            {
                frame = Quantization(obj, null, palette, paletteStartIndex, paletteEndIndex);
            }

            yield return frame.TrimAndCompress();
        }

        if (!frames.Any(i => i.Shadow is not null))
            yield break;

        foreach (var src in frames)
        {
            ShapeImageFrame frame;
            if (src.Shadow is { Exists: true })
            {
                using var sha = SKBitmap.Decode(src.Shadow.FullName);
                frame = Shadow(sha).TrimAndCompress();
            }
            else
            {
                using var obj = SKBitmap.Decode(src.Object.FullName);
                frame = new(new()
                {
                    X = 0,
                    Y = 0,
                    Width = (ushort)obj.Width,
                    Height = (ushort)obj.Height,
                    CompressionType = ShapeFrameCompressionType.None
                }, new byte[obj.Width * obj.Height]);
            }
            yield return frame.TrimAndCompress();
        }
    }

    private static ShapeImageFrame Shadow(SKBitmap sha)
    {
        using MemoryStream output = new();

        for (int y = 0; y < sha.Height; y++)
        {
            for (int x = 0; x < sha.Width; x++)
            {
                SKColor pixel = sha.GetPixel(x, y);
                output.WriteByte(pixel.Alpha is 0 ? (byte)0 : (byte)1);
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

    /// <summary>
    /// 量化颜色
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="col"></param>
    /// <param name="palette"></param>
    /// <param name="paletteStartIndex"></param>
    /// <param name="paletteEndIndex"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    private static ShapeImageFrame Quantization(SKBitmap obj, SKBitmap? col, Palette palette, int paletteStartIndex, int paletteEndIndex)
    {
        using MemoryStream output = new();

        if (col is not null && (obj.Width != col.Width || obj.Height != col.Height))
            throw new FormatException();

        if (paletteStartIndex < 0)
            paletteStartIndex = col is null ? 16 : 32;

        Func<int, int, byte> getIndex = col is null
            ? (x, y) => GetIndex(palette, obj.GetPixel(x, y), (byte)paletteStartIndex, (byte)paletteEndIndex)
            : (x, y) => col.GetPixel(x, y) is { Alpha: not 0 } cp
                ? GetHouseIndex(palette, cp)
                : GetIndex(palette, obj.GetPixel(x, y), (byte)paletteStartIndex, (byte)paletteEndIndex);

        for (int y = 0; y < obj.Height; y++)
            for (int x = 0; x < obj.Width; x++)
                output.WriteByte(getIndex(x, y));

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
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private static byte GetIndex(in Palette palette, in SKColor pixel, byte start, byte end)
    {
        if (pixel.Alpha is 0)
            return 0;

        double cdistance = double.MaxValue;
        byte index = 0;
        for (byte i = start; i < end; i++)
        {
            DisplayColor color = palette[i];

            double distance = Math.Sqrt(Math.Pow(color.Red - pixel.Red, 2) + Math.Pow(color.Green - pixel.Green, 2) + Math.Pow(color.Blue - pixel.Blue, 2));
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
    private static byte GetHouseIndex(in Palette palette, in SKColor pixel)
    {
        if (pixel.Alpha is 0)
            return 0;

        double cdistance = double.MaxValue;
        byte index = 0;
        for (byte i = 16; i < 32; i++)
        {
            DisplayColor color = palette[i];
            var gray = (color.Red - pixel.Red) + (color.Green - pixel.Green) + (color.Blue - pixel.Blue);

            double distance = Math.Sqrt(Math.Pow(gray, 2));
            if (distance < cdistance)
            {
                index = i;
                cdistance = distance;
            }
        }

        return index;
    }
}
