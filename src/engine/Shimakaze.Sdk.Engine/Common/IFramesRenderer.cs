namespace Shimakaze.Sdk.Engine.Common;

internal interface IFramesRenderer
{
    Renderer GetFrame(int index);
}

internal interface IFramesRenderer<out TRenderer> : IFramesRenderer
    where TRenderer : Renderer
{
    new TRenderer GetFrame(int index);
}
