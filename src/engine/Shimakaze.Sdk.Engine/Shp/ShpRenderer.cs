using System.Drawing;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

internal sealed class ShpRenderer(ShapeImage shape, Palette palette) : FramesRenderer<ShpFrameRenderer>
{
    private readonly int _half = shape.Frames.Count / 2;

    public Size Size { get; } = new(shape.Metadata.Width, shape.Metadata.Height);
    public override int Count => HasShadow ? _half : Shape.Frames.Count;

    public ShapeImage Shape { get; } = shape;
    public bool UseTransparent { get; set; }
    public bool HasShadow { get; set => field = Shape.Frames.Count % 2 is 0 && value; }
    public BGRA32[]? HouseColors { get; set; }

    public readonly BGRA32[] Palette = [.. palette.Cast<DisplayColor>().Select(i => (BGRA32)i)];

    public override ShpFrameRenderer GetFrame(int index)
    {
        var obj = Shape.Frames[index];
        if (HasShadow)
            return new ShpShadowedFrameRenderer(this, obj, Shape.Frames[_half + index]);

        return new(this, obj);
    }
}
