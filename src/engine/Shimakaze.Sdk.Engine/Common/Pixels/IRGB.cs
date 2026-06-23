namespace Shimakaze.Sdk.Engine.Common.Pixels;

/// <summary>
/// Defines a pixel type with red, green, and blue color channels.
/// </summary>
public interface IRGB
{
    /// <summary>
    /// Gets the red component.
    /// </summary>
    byte R { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    byte G { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    byte B { get; }
}

/// <summary>
/// Provides extension methods for converting RGB pixel types to HSV.
/// </summary>
public static class RGBExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IRGB"/> types.
    /// </summary>
    /// <typeparam name="TRGB">The pixel type that implements <see cref="IRGB"/>.</typeparam>
    extension<TRGB>(TRGB rgb)
        where TRGB : unmanaged, IRGB
    {
        /// <summary>
        /// Converts the RGB color to its HSV representation.
        /// </summary>
        /// <returns>
        /// An <see cref="HSV"/> value with H in the range [0, 360), S in [0, 1], and V in [0, 1].
        /// </returns>
        public HSV ToHSV()
        {
            float r = rgb.R / 255f;
            float g = rgb.G / 255f;
            float b = rgb.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0, s, v = max;

            // Compute saturation S
            s = (max == 0) ? 0 : delta / max;

            // Compute hue H
            if (delta < 1e-6) // Gray or black
                h = 0;
            else if (Math.Abs(max - r) < 1e-6)
                h = 60 * ((g - b) / delta % 6);
            else if (Math.Abs(max - g) < 1e-6)
                h = 60 * (((b - r) / delta) + 2);
            else if (Math.Abs(max - b) < 1e-6)
                h = 60 * (((r - g) / delta) + 4);

            // Ensure H is in the [0, 360) range
            if (h < 0) h += 360;

            return new(h, s, v);
        }
    }
}
