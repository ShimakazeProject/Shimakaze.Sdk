namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Defines a renderer that can produce frames as renderable images.
/// </summary>
public interface IFramesRenderer
{
    /// <summary>
    /// Gets the renderer for the frame at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the frame.</param>
    /// <returns>The renderer for the specified frame.</returns>
    Renderer GetFrame(int index);
}

/// <summary>
/// Defines a generic frame-based renderer with a specific renderer type.
/// </summary>
/// <typeparam name="TRenderer">The type of renderer used for each frame.</typeparam>
public interface IFramesRenderer<out TRenderer> : IFramesRenderer
    where TRenderer : Renderer
{
    /// <summary>
    /// Gets the renderer for the frame at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the frame.</param>
    /// <returns>The renderer for the specified frame.</returns>
    new TRenderer GetFrame(int index);
}
