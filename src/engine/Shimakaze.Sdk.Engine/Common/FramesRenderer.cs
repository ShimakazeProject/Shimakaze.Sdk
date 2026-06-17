namespace Shimakaze.Sdk.Engine.Common;

internal abstract class FramesRenderer<TRenderer> : IFramesRenderer<TRenderer>
    where TRenderer : Renderer
{
    public abstract int Count { get; }

    public abstract TRenderer GetFrame(int index);

    Renderer IFramesRenderer.GetFrame(int index) => GetFrame(index);
}
