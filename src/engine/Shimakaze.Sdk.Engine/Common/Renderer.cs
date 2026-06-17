using System.Drawing;

namespace Shimakaze.Sdk.Engine.Common;

internal abstract class Renderer
{
    public abstract Size Size { get; }

    public virtual BGRA32[] CreateBuffer() => GC.AllocateUninitializedArray<BGRA32>(Size.Width * Size.Height);

    public abstract void RenderTo(BGRA32[] canvas);

    public Image RenderAsImage()
    {
        var buffer = CreateBuffer();
        RenderTo(buffer);
        return new(Size, buffer);
    }
}
