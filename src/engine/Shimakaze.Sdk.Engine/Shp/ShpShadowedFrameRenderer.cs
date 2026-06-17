using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Shp;

internal class ShpShadowedFrameRenderer(ShpRenderer shpRenderer, ShapeImageFrame @object, ShapeImageFrame shadow) : ShpFrameRenderer(shpRenderer, @object)
{
    public override void RenderTo(BGRA32[] canvas)
    {
        RenderTo(shadow, canvas);
        base.RenderTo(canvas);
    }
}
