using System.Drawing;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

/// <summary>
/// Renders a <see cref="ShapeImage"/> as a sequence of frames.
/// <br />
/// Supports optional shadow rendering when the SHP contains an even number of frames
/// (the second half are treated as shadow frames paired with the first half).
/// </summary>
/// <param name="shape">The SHP image to render.</param>
/// <param name="palette">The colour palette for pixel lookup.</param>
public sealed class ShapeRenderer(ShapeImage shape, Palette palette) : FramesRenderer<ShapeFrameRenderer>
{
    private readonly int _half = shape.Frames.Count / 2;

    /// <summary>
    /// Gets the frame size in pixels, derived from the SHP metadata.
    /// </summary>
    public Size Size { get; } = new(shape.Metadata.Width, shape.Metadata.Height);

    /// <summary>
    /// Gets the number of visible frames.
    /// <br />
    /// When <see cref="HasShadow"/> is <see langword="true"/>, only the first half
    /// (object frames) are counted.
    /// </summary>
    public override int Count => HasShadow ? _half : Shape.Frames.Count;

    /// <summary>
    /// Gets the underlying <see cref="ShapeImage"/> being rendered.
    /// </summary>
    public ShapeImage Shape { get; } = shape;

    /// <summary>
    /// Gets or sets whether shadow frames are enabled.
    /// <br />
    /// Only valid when the total frame count is even.
    /// </summary>
    public bool HasShadow { get; set => field = Shape.Frames.Count % 2 is 0 && value; }

    /// <summary>
    /// The palette converted to an array of <see cref="BGRA32"/> pixels for rendering.
    /// </summary>
    public BGRA32[] Palette { get; } = [.. palette.Select(i => (BGRA32)i)];

    /// <inheritdoc/>
    public override ShapeFrameRenderer GetFrame(int index)
    {
        var obj = Shape.Frames[index];
        if (HasShadow)
            return new ShapeShadowedFrameRenderer(this, obj, Shape.Frames[_half + index]);

        return new(this, obj);
    }

    /// <summary>
    /// Updates the house colour in the palette (indices 16–31) to the specified colour.
    /// <br />
    /// Preserves the saturation and value of each palette entry while replacing the hue.
    /// </summary>
    /// <param name="color">The new house colour.</param>
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
