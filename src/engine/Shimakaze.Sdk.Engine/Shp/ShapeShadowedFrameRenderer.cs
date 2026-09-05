using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

/// <summary>
/// Renders a single SHP frame with a shadow overlay.
/// <br />
/// Renders the shadow frame first (only non-zero pixels), then the object frame on top.
/// </summary>
/// <param name="shpRenderer">The parent <see cref="ShapeRenderer"/> that owns this frame.</param>
/// <param name="object">The object frame to render.</param>
/// <param name="shadow">The shadow frame to render underneath the object.</param>
public class ShapeShadowedFrameRenderer(ShapeRenderer shpRenderer, ShapeImageFrame @object, ShapeImageFrame shadow) : ShapeFrameRenderer(shpRenderer, @object)
{
    /// <inheritdoc/>
    protected override void RenderTo(byte[] indexes)
    {
        RenderTo(shadow, indexes);
        base.RenderTo(indexes);
    }

    /// <inheritdoc/>
    protected override void RenderTo(ShapeImageFrame frame, byte[] indexes)
    {
        for (int y = 0; y < frame.Metadata.Height; y++)
        {
            int i = y + frame.Metadata.Y;
            var span = indexes.AsSpan((i * Size.Width) + frame.Metadata.X, frame.Metadata.Width);
            var row = frame.Indexes.Slice(y * frame.Metadata.Width, frame.Metadata.Width).Span;
            for (int j = 0; j < row.Length; j++)
            {
                if (row[j] is byte b and not 0)
                    span[j] = b;
            }
        }
    }
}
