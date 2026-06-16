using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

internal sealed class ShpExtractor(ShapeImage shape, Palette palette)
{
    public ShapeImage Shape { get; } = shape;
    public Palette Palette { get; } = palette;

    private readonly Dictionary<byte, BGRA32> _paletteCache = new(palette.Colors.Length);

    private static BGRA32 ToColor(DisplayColor color) => new(color.Blue, color.Green, color.Red, byte.MaxValue);

    public void SetColor(byte index, BGRA32 color)
    {
        _paletteCache[index] = color;
    }

    private BGRA32 GetColor(byte index)
    {
        if (!_paletteCache.TryGetValue(index, out var color))
            _paletteCache[index] = color = ToColor(Palette[index]);

        return color;
    }

    public BGRA32[] CreateCanvas(bool useAlpha)
    {
        BGRA32 bg = useAlpha
            ? BGRA32.Transparent
            : GetColor(0);

        BGRA32[] data = GC.AllocateUninitializedArray<BGRA32>(Shape.Metadata.Width * Shape.Metadata.Height);
        data.AsSpan().Fill(bg);
        return data;
    }

    public void DrawFrame(BGRA32[] canvas, ShapeImageFrame frame, PaletteColor[] houseColors)
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
