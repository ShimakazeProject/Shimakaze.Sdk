namespace Shimakaze.Sdk.Engine.Common.Pixels;

/// <summary>
/// Represents a color in the HSV (Hue, Saturation, Value) color space.
/// </summary>
/// <param name="H">The hue component (0 - 360).</param>
/// <param name="S">The saturation component (0 - 1).</param>
/// <param name="V">The value (brightness) component (0 - 1).</param>
public readonly record struct HSV(float H, float S, float V)
{
    /// <summary>
    /// Converts the HSV color to the specified pixel type.
    /// </summary>
    /// <typeparam name="TPixel">The target pixel type that supports conversion from HSV.</typeparam>
    /// <returns>The converted pixel value.</returns>
    public TPixel To<TPixel>()
        where TPixel : unmanaged, IFromHSV<TPixel>
    {
#if NET7_0_OR_GREATER
        return TPixel.FromHSV(this);
#else
        TPixel pixel = default;
        return pixel.FromHSV(this);
#endif
    }
}
