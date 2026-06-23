namespace Shimakaze.Sdk.Engine.Common.Pixels;

/// <summary>
/// Defines a pixel type that can be constructed from an HSV color value.
/// </summary>
/// <typeparam name="TPixel">The pixel type that implements this interface.</typeparam>
public interface IFromHSV<out TPixel> where TPixel : unmanaged, IFromHSV<TPixel>
{
    /// <summary>
    /// Creates a pixel value from an HSV color.
    /// </summary>
    /// <param name="hsv">The HSV color value to convert from.</param>
    /// <returns>The converted pixel value.</returns>
#if NET7_0_OR_GREATER
    static abstract
#endif
    TPixel FromHSV(in HSV hsv);
}
