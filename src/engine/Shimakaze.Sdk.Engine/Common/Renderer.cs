using System.Drawing;

namespace Shimakaze.Sdk.Engine.Common;

internal abstract class Renderer
{
    public virtual Size Size { get; protected init; }

    public virtual BGRA32[] CreateBuffer() => GC.AllocateUninitializedArray<BGRA32>(Size.Width * Size.Height);

    public abstract void Render(BGRA32[] canvas);

    public Image Render()
    {
        var buffer = CreateBuffer();
        Render(buffer);
        return new(Size, buffer);
    }
}
