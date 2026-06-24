using System.Drawing;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

/// <summary>
/// Renders a single <see cref="ShapeImageFrame"/> as a <see cref="PaletteImage"/>.
/// </summary>
/// <param name="shpRenderer">The parent <see cref="ShapeRenderer"/> that owns this frame.</param>
/// <param name="frame">The SHP frame to render.</param>
public class ShapeFrameRenderer(ShapeRenderer shpRenderer, ShapeImageFrame frame) : Renderer
{
    /// <inheritdoc/>
    public override Size Size => shpRenderer.Size;

    /// <inheritdoc/>
    public override Image RenderAsImage()
    {
        byte[] indexes = new byte[Size.Width * Size.Height];

        RenderTo(indexes);

        return new PaletteImage(
            Size.Width,
            Size.Height,
            ImmutableCollectionsMarshal.AsImmutableArray(shpRenderer.Palette),
            ImmutableCollectionsMarshal.AsImmutableArray(indexes));
    }

    /// <inheritdoc/>
    protected virtual void RenderTo(byte[] indexes) => RenderTo(frame, indexes);

    /// <inheritdoc/>
    protected virtual void RenderTo(ShapeImageFrame frame, byte[] indexes)
    {
        for (int y = 0; y < frame.Metadata.Height; y++)
        {
            int i = y + frame.Metadata.Y;
            var span = indexes.AsSpan((i * Size.Width) + frame.Metadata.X, frame.Metadata.Width);
            var row = frame.Indexes.Slice(y * frame.Metadata.Width, frame.Metadata.Width).Span;
            row.CopyTo(span);
        }
    }
}
