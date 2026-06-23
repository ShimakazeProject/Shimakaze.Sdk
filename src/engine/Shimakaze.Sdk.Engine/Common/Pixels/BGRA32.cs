using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Common.Pixels;

/// <summary>
/// Represents a 32-bit BGRA color value with blue, green, red, and alpha components.
/// </summary>
/// <param name="B">The blue component.</param>
/// <param name="G">The green component.</param>
/// <param name="R">The red component.</param>
/// <param name="A">The alpha (transparency) component.</param>
public readonly record struct BGRA32(byte B, byte G, byte R, byte A) : IRGB, IFromHSV<BGRA32>
{
    /// <summary>
    /// A fully transparent BGRA color.
    /// </summary>
    public static readonly BGRA32 Transparent = new(0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="BGRA32"/> struct with full opacity.
    /// </summary>
    /// <param name="b">The blue component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="r">The red component.</param>
    public BGRA32(byte b, byte g, byte r) : this(b, g, r, byte.MaxValue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BGRA32"/> struct from a packed 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The packed color value in BGRA byte order (B in MSB, A in LSB).</param>
    public BGRA32(uint value) : this((byte)((value & 0xFF000000) >> 24), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF))
    {
    }

#if !NET7_0_OR_GREATER
    BGRA32 IFromHSV<BGRA32>.FromHSV(in HSV hsv) => FromHSV(hsv);
#endif

    /// <summary>
    /// Creates a <see cref="BGRA32"/> color from an HSV value.
    /// </summary>
    /// <param name="hsv">The HSV color value.</param>
    /// <returns>A <see cref="BGRA32"/> color with full opacity.</returns>
    public static BGRA32 FromHSV(in HSV hsv)
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

        return new(blue, green, red);
    }

    /// <summary>
    /// Computes the bitwise complement (inverse) of the color, preserving alpha.
    /// </summary>
    /// <param name="color">The color to invert.</param>
    /// <returns>The inverted <see cref="BGRA32"/> color.</returns>
    public static BGRA32 operator ~(BGRA32 color)
        => new((byte)~color.B, (byte)~color.G, (byte)~color.R);

    /// <summary>
    /// Implicitly converts a <see cref="PaletteColor"/> to a <see cref="BGRA32"/>.
    /// </summary>
    /// <param name="color">The palette color to convert.</param>
    public static implicit operator BGRA32(PaletteColor color) => new(color.ExpandedB, color.ExpandedG, color.ExpandedR);

    /// <summary>
    /// Explicitly converts a <see cref="RGBA32"/> to a <see cref="BGRA32"/> by swapping the red and blue channels.
    /// </summary>
    /// <param name="c">The RGBA color to convert.</param>
    public static explicit operator BGRA32(RGBA32 c) => new(c.B, c.G, c.R, c.A);
}
