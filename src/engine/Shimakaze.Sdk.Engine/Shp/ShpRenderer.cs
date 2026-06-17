using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

internal sealed class ShpRenderer(ShapeImage shape, Palette palette)
{
    public ShapeImage Shape { get; } = shape;

    private readonly BGRA32[] _palette = [.. palette.Cast<DisplayColor>().Select(i => (BGRA32)i)];

    public void SetColor(byte index, BGRA32 color) => _palette[index] = color;

    public BGRA32[] CreateCanvas(bool useAlpha)
    {
        BGRA32 bg = useAlpha
            ? BGRA32.Transparent
            : _palette[0];

        BGRA32[] data = GC.AllocateUninitializedArray<BGRA32>(Shape.Metadata.Width * Shape.Metadata.Height);
        data.AsSpan().Fill(bg);
        return data;
    }

    public void DrawFrame(BGRA32[] canvas, ShapeImageFrame frame, BGRA32[] houseColors)
    {
        ReadOnlySpan<BGRA32> house = houseColors;

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
                    >= 16 and < 32 when !house.IsEmpty => house[index - 16],
                    _ => _palette[index],
                };
            }
        }
    }
}
