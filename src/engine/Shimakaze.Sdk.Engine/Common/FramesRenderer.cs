namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Provides an abstract base class for frame-based renderers.
/// </summary>
/// <typeparam name="TRenderer">The type of renderer used for each frame.</typeparam>
public abstract class FramesRenderer<TRenderer> : IFramesRenderer<TRenderer>
    where TRenderer : Renderer
{
    /// <summary>
    /// Gets the number of frames.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets the renderer for the frame at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the frame.</param>
    /// <returns>The renderer for the specified frame.</returns>
    public abstract TRenderer GetFrame(int index);

    /// <inheritdoc />
    Renderer IFramesRenderer.GetFrame(int index) => GetFrame(index);
}
