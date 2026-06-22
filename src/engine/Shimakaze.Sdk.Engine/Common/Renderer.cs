using System.Drawing;

namespace Shimakaze.Sdk.Engine.Common;

internal abstract class Renderer
{
    public abstract Size Size { get; }

    public abstract Image RenderAsImage();
}
