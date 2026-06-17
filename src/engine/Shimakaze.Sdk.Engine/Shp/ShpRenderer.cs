using System.Drawing;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Engine.Common.Pixels;
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

    public readonly BGRA32[] Palette = [.. palette.Cast<DisplayColor>().Select(i => (BGRA32)i)];

    public override ShpFrameRenderer GetFrame(int index)
    {
        var obj = Shape.Frames[index];
        if (HasShadow)
            return new ShpShadowedFrameRenderer(this, obj, Shape.Frames[_half + index]);

        return new(this, obj);
    }

    public void UpdateHouseColor(BGRA32 color)
    {
        UpdateHouse(16, color);
        UpdateHouse(17, color);
        UpdateHouse(18, color);
        UpdateHouse(19, color);
        UpdateHouse(20, color);
        UpdateHouse(21, color);
        UpdateHouse(22, color);
        UpdateHouse(23, color);
        UpdateHouse(24, color);
        UpdateHouse(25, color);
        UpdateHouse(26, color);
        UpdateHouse(27, color);
        UpdateHouse(28, color);
        UpdateHouse(29, color);
        UpdateHouse(30, color);
        UpdateHouse(31, color);
    }

    private void UpdateHouse(int index, BGRA32 color)
    {
        var (h, _, _) = color.ToHSV();
        var (_, s, v) = Palette[index].ToHSV();
        Palette[index] = BGRA32.FromHSV(new(h, s, v));
    }
}
