using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// Represents a palette color entry with R, G, B components.
/// Primarily used to describe a compact 18-bit color (6 bits per channel, RGB666),
/// but can also hold a full RGB24 color.
/// </summary>
/// <remarks>
/// <see href="https://modenc.renegadeprojects.com/PAL"/>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct PaletteColor(byte R, byte G, byte B)
{
    /// <summary>
    /// Bits per pixel for unpacked (RGB24) format.
    /// </summary>
    public const int BitPerPixel = BytePerPixel * 8;

    /// <summary>
    /// Bytes per pixel (3 bytes, one per R/G/B component).
    /// </summary>
    public const int BytePerPixel = sizeof(byte) * 3;

    /// <summary>
    /// Red component.
    /// </summary>
    public byte R { readonly get; set; } = R;
    /// <summary>
    /// Green component.
    /// </summary>
    public byte G { readonly get; set; } = G;
    /// <summary>
    /// Blue component.
    /// </summary>
    public byte B { readonly get; set; } = B;

    /// <summary>
    /// Red component expanded from 6-bit to 8-bit range.
    /// When in compact 18-bit mode, left-shifts by 2; otherwise returns <see cref="R"/> as-is.
    /// </summary>
    public readonly byte ExpandedR => IsPaletteColor ? (byte)(R << 2) : R;
    /// <summary>
    /// Green component expanded from 6-bit to 8-bit range.
    /// When in compact 18-bit mode, left-shifts by 2; otherwise returns <see cref="G"/> as-is.
    /// </summary>
    public readonly byte ExpandedG => IsPaletteColor ? (byte)(G << 2) : G;
    /// <summary>
    /// Blue component expanded from 6-bit to 8-bit range.
    /// When in compact 18-bit mode, left-shifts by 2; otherwise returns <see cref="B"/> as-is.
    /// </summary>
    public readonly byte ExpandedB => IsPaletteColor ? (byte)(B << 2) : B;

    /// <summary>
    /// Indicates whether the color is stored in compact 18-bit (RGB666) format.
    /// Returns <see langword="true"/> when the upper 2 bits of R, G, and B are all zero,
    /// meaning each component only occupies the lower 6 bits (value range 0–63).
    /// </summary>
    public readonly bool IsPaletteColor => (R & 0b11000000, G & 0b11000000, B & 0b11000000) is (0, 0, 0);

    #region Backward compatibility (to be removed)

    /// <summary>
    /// Gets the red component. Prefer <see cref="R"/>.
    /// </summary>
    [Obsolete("Use R instead.")]
    public readonly byte Red => R;

    /// <summary>
    /// Gets the green component. Prefer <see cref="G"/>.
    /// </summary>
    [Obsolete("Use G instead.")]
    public readonly byte Green => G;

    /// <summary>
    /// Gets the blue component. Prefer <see cref="B"/>.
    /// </summary>
    [Obsolete("Use B instead.")]
    public readonly byte Blue => B;
    #endregion

    /// <summary>
    /// Masks each component to the lower 6 bits (0–63 range), enforcing compact 18-bit form.
    /// Calls to this method guarantee <see cref="IsPaletteColor"/> returns <see langword="true"/> afterward.
    /// </summary>
    public void TruncateHighBits()
    {
        R = (byte)(R & 0b00111111);
        G = (byte)(G & 0b00111111);
        B = (byte)(B & 0b00111111);
        Debug.Assert(IsPaletteColor);
    }

    /// <summary>
    /// Converts from RGB24 down to compact 18-bit format by right-shifting each component by 2.
    /// No operation if already in compact form.
    /// </summary>
    public void ConvertToPaletteColor()
    {
        if (IsPaletteColor)
            return;

        R = (byte)(R >> 2);
        G = (byte)(G >> 2);
        B = (byte)(B >> 2);
        Debug.Assert(IsPaletteColor);
    }

    /// <summary>
    /// Expands from compact 18-bit format up to RGB24 by left-shifting each component by 2.
    /// No operation if already in RGB24 form.
    /// </summary>
    public void ExpandToRgb24()
    {
        if (!IsPaletteColor)
            return;

        R = (byte)(R << 2);
        G = (byte)(G << 2);
        B = (byte)(B << 2);
        Debug.Assert(!IsPaletteColor);
    }

    /// <summary>
    /// Computes the bitwise complement of the color.
    /// Each component is independently inverted: ~R, ~G, ~B.
    /// </summary>
    public static PaletteColor operator ~(PaletteColor color)
        => new((byte)~color.R, (byte)~color.G, (byte)~color.B);

    /// <summary>
    /// Returns the color as a <c>#RRGGBB</c> hex string.
    /// </summary>
    public override readonly string ToString() => $"#{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// Converts this color to a <see cref="Color"/> value using the raw R, G, B components.
    /// </summary>
    public readonly Color ToColor() => Color.FromArgb(R, G, B);

    /// <summary>
    /// Creates a <see cref="PaletteColor"/> from a 24-bit RGB888 integer value.
    /// </summary>
    public static PaletteColor Create(int rgb888) => Create((uint)rgb888);

    /// <inheritdoc cref="Create(int)"/>
    public static PaletteColor Create(uint rgb888)
    {
        byte r = (byte)((rgb888 & 0xFF0000) >> 16);
        byte g = (byte)((rgb888 & 0x00FF00) >> 8);
        byte b = (byte)((rgb888 & 0x0000FF) >> 0);
        return new(r, g, b);
    }

    /// <summary>
    /// Creates a <see cref="PaletteColor"/> from a 16-bit RGB565 integer value.
    /// R and B are extracted from 5 bits each; G from 6 bits.
    /// </summary>
    public static PaletteColor Create(short rgb565) => Create((ushort)rgb565);

    /// <inheritdoc cref="Create(short)"/>
    public static PaletteColor Create(ushort rgb565)
    {
        byte r = (byte)((rgb565 & 0b11111_000000_00000) >> 11);
        byte g = (byte)((rgb565 & 0b00000_111111_00000) >> 5);
        byte b = (byte)((rgb565 & 0b00000_000000_11111) >> 0);
        return new(r, g, b);
    }

    /// <summary>
    /// Creates a <see cref="PaletteColor"/> from a <see cref="Color"/> instance,
    /// using the raw R, G, B component values.
    /// </summary>
    public static PaletteColor Create(Color color) => new(color.R, color.G, color.B);
}
