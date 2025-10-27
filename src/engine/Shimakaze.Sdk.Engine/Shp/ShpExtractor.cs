using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using SkiaSharp;

namespace Shimakaze.Sdk.Engine.Shp;

internal sealed class ShpExtractor(ShapeImage shape, Palette palette)
{
    public ShapeImage Shape { get; } = shape;
    public Palette Palette { get; } = palette;

    private readonly Dictionary<byte, SKColor> _paletteCache = new(palette.Colors.Length);

    private static SKColor ToColor(DisplayColor color) => new(color.Red, color.Green, color.Blue, byte.MaxValue);

    public void SetColor(byte index, SKColor color)
    {
        _paletteCache[index] = color;
    }

    private SKColor GetColor(byte index)
    {
        if (!_paletteCache.TryGetValue(index, out var color))
            _paletteCache[index] = color = ToColor(Palette[index]);

        return color;
    }

    public SKColor[] CreateCanvas(bool useAlpha)
    {
        SKColor bg = useAlpha
            ? new(0, 0, 0, 0)
            : GetColor(0);

        SKColor[] data = new SKColor[Shape.Metadata.Width * Shape.Metadata.Height];
        data.AsSpan().Fill(bg);
        return data;
    }

    public void DrawFrame(SKColor[] canvas, ShapeImageFrame frame, PaletteColor[] houseColors)
    {
        ReadOnlySpan<PaletteColor> house = houseColors;

        for (int y = 0; y < frame.Metadata.Height; y++)
        {
            var i = y + frame.Metadata.Y;
            var span = canvas.AsSpan(i * Shape.Metadata.Width + frame.Metadata.X, frame.Metadata.Width);
            var row = frame.Indexes.Slice((y * frame.Metadata.Width), frame.Metadata.Width).Span;

            for (int j = 0; j < row.Length; j++)
            {
                var index = row[j];
                if (index is 0)
                    continue;

                span[j] = index switch
                {
                    >= 16 and < 32 when !house.IsEmpty => ToColor(house[index - 16]),
                    _ => GetColor(index),
                };
            }
        }
    }
}
