using System.Drawing;

namespace Shimakaze.Sdk.Engine.Common;

/// <summary>
/// Provides an abstract base class for renderers that produce images.
/// </summary>
public abstract class Renderer
{
    /// <summary>
    /// Gets the size of the rendered output.
    /// </summary>
    public abstract Size Size { get; }

    /// <summary>
    /// Renders the content as an <see cref="Image"/>.
    /// </summary>
    /// <returns>The rendered image.</returns>
    public abstract Image RenderAsImage();
}
