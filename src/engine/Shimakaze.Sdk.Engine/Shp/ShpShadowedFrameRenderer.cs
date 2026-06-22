using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

internal class ShpShadowedFrameRenderer(ShpRenderer shpRenderer, ShapeImageFrame @object, ShapeImageFrame shadow) : ShpFrameRenderer(shpRenderer, @object)
{
    protected override void RenderTo(byte[] indexes)
    {
        RenderTo(shadow, indexes);
        base.RenderTo(indexes);
    }

    protected override void RenderTo(ShapeImageFrame frame, byte[] indexes)
    {
        for (int y = 0; y < frame.Metadata.Height; y++)
        {
            int i = y + frame.Metadata.Y;
            var span = indexes.AsSpan((i * Size.Width) + frame.Metadata.X, frame.Metadata.Width);
            var row = frame.Indexes.Slice(y * frame.Metadata.Width, frame.Metadata.Width).Span;
            for (var j = 0; j < row.Length; j++)
            {
                if (row[j] is byte b and not 0)
                    span[j] = b;
            }
        }
    }
}
