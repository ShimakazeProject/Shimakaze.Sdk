using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Common.Pixels;

/// <summary>
/// Represents a 32-bit RGBA color value with red, green, blue, and alpha components.
/// </summary>
/// <param name="R">The red component.</param>
/// <param name="G">The green component.</param>
/// <param name="B">The blue component.</param>
/// <param name="A">The alpha (transparency) component.</param>
public readonly record struct RGBA32(byte R, byte G, byte B, byte A) : IRGB, IFromHSV<RGBA32>
{
    /// <summary>
    /// A fully transparent RGBA color.
    /// </summary>
    public static readonly RGBA32 Transparent = new(0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="RGBA32"/> struct with full opacity.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    public RGBA32(byte r, byte g, byte b) : this(r, g, b, byte.MaxValue)
    {
    }

#if !NET7_0_OR_GREATER
    RGBA32 IFromHSV<RGBA32>.FromHSV(in HSV hsv) => FromHSV(hsv);
#endif

    /// <summary>
    /// Creates an <see cref="RGBA32"/> color from an HSV value.
    /// </summary>
    /// <param name="hsv">The HSV color value.</param>
    /// <returns>An <see cref="RGBA32"/> color with full opacity.</returns>
    public static RGBA32 FromHSV(in HSV hsv)
    {
        float c = hsv.V * hsv.S;             // 色度
        float x = c * (1 - Math.Abs((hsv.H / 60 % 2) - 1));
        float m = hsv.V - c;             // 亮度偏移量

        float r = 0, g = 0, b = 0;

        if (hsv.H is >= 0 and < 60) { r = c; g = x; }
        else if (hsv.H < 120) { r = x; g = c; }
        else if (hsv.H < 180) { g = c; b = x; }
        else if (hsv.H < 240) { g = x; b = c; }
        else if (hsv.H < 300) { r = x; b = c; }
        else if (hsv.H < 360) { r = c; b = x; }

        byte red = (byte)Math.Round((r + m) * 255);
        byte green = (byte)Math.Round((g + m) * 255);
        byte blue = (byte)Math.Round((b + m) * 255);

        return new(red, green, blue);
    }

    /// <summary>
    /// Computes the bitwise complement (inverse) of the color, preserving alpha.
    /// </summary>
    /// <param name="color">The color to invert.</param>
    /// <returns>The inverted <see cref="RGBA32"/> color.</returns>
    public static RGBA32 operator ~(RGBA32 color)
        => new((byte)~color.R, (byte)~color.G, (byte)~color.B);

    /// <summary>
    /// Implicitly converts a <see cref="PaletteColor"/> to an <see cref="RGBA32"/>.
    /// </summary>
    /// <param name="color">The palette color to convert.</param>
    public static implicit operator RGBA32(PaletteColor color) => new(color.ExpandedR, color.ExpandedG, color.ExpandedB);

    /// <summary>
    /// Explicitly converts a <see cref="BGRA32"/> to an <see cref="RGBA32"/> by swapping the red and blue channels.
    /// </summary>
    /// <param name="c">The BGRA color to convert.</param>
    public static explicit operator RGBA32(BGRA32 c) => new(c.R, c.G, c.B, c.A);
}
