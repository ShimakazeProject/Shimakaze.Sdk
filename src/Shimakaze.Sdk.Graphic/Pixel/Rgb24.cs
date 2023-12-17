using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Graphic.Pixel;

/// <summary>
/// 24位色
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
public readonly record struct Rgb24 : IPixel
{
    /// <inheritdoc cref="Rgba32.Size"/>
    public const int Size = sizeof(byte) * 3;
    /// <inheritdoc cref="Rgba32.Red"/>
    public readonly byte Red;
    /// <inheritdoc cref="Rgba32.Green"/>
    public readonly byte Green;
    /// <inheritdoc cref="Rgba32.Blue"/>
    public readonly byte Blue;

    /// <inheritdoc cref="Rgba32()"/>
    public Rgb24()
    {
    }

    /// <inheritdoc cref="Rgba32(byte, byte, byte)"/>
    public Rgb24(byte red, byte green, byte blue) : this()
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    /// <inheritdoc/>
    public static IPixel FromRgb24(in Rgb24 pixel) => new Rgb24(pixel.Red, pixel.Green, pixel.Blue);

    /// <inheritdoc/>
    public static IPixel FromRgb565(in Rgb565 pixel) => new Rgb24(pixel.Red, pixel.Green, pixel.Blue);

    /// <inheritdoc/>
    public static IPixel FromRgba32(in Rgba32 pixel) => new Rgb24(pixel.Red, pixel.Green, pixel.Blue);


    /// <inheritdoc/>
    public void ToRgb24(out Rgb24 pixel) => pixel = new(Red, Green, Blue);

    /// <inheritdoc/>
    public void ToRgb565(out Rgb565 pixel) => pixel = Rgb565.FromGameColor(this);

    /// <inheritdoc/>
    public void ToRgba32(out Rgba32 pixel) => pixel = new(Red, Green, Blue);

    /// <inheritdoc />
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";
}
