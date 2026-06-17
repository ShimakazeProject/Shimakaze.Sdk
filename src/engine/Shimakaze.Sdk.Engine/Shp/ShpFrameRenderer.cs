using System.Drawing;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

internal class ShpFrameRenderer(ShpRenderer shpRenderer, ShapeImageFrame frame) : Renderer
{
    public override Size Size => shpRenderer.Size;
    public bool UseTransparent => shpRenderer.UseTransparent;
    public override BGRA32[] CreateBuffer()
    {
        var buffer = base.CreateBuffer();
        var bg = UseTransparent
            ? BGRA32.Transparent
            : shpRenderer.Palette[0];
        buffer.AsSpan().Fill(bg);
        return buffer;
    }

    public override void RenderTo(BGRA32[] canvas) => RenderTo(frame, canvas);

    protected void RenderTo(ShapeImageFrame frame, BGRA32[] canvas)
    {
        ReadOnlySpan<BGRA32> house = shpRenderer.HouseColors;

        for (int y = 0; y < frame.Metadata.Height; y++)
        {
            int i = y + frame.Metadata.Y;
            var span = canvas.AsSpan((i * Size.Width) + frame.Metadata.X, frame.Metadata.Width);
            var row = frame.Indexes.Slice(y * frame.Metadata.Width, frame.Metadata.Width).Span;

            for (int j = 0; j < row.Length; j++)
            {
                byte index = row[j];
                if (index is 0)
                    continue;

                span[j] = index switch
                {
                    >= 16 and < 32 when !house.IsEmpty => house[index - 16],
                    _ => shpRenderer.Palette[index],
                };
            }
        }
    }
}
