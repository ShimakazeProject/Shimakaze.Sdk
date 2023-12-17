using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Graphic.Pixel;

/// <summary>
/// 32位色
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
public readonly record struct Rgba32 : IPixel
{
    /// <summary>
    /// 字节每像素（色彩深度）
    /// </summary>
    public const int Size = sizeof(byte) * 4;
    /// <summary>
    /// 红色
    /// </summary>
    public readonly byte Red;
    /// <summary>
    /// 绿色
    /// </summary>
    public readonly byte Green;
    /// <summary>
    /// 蓝色
    /// </summary>
    public readonly byte Blue;
    /// <summary>
    /// 透明度
    /// </summary>
    public readonly byte Alpha;

    /// <summary>
    /// 创建一个颜色
    /// </summary>
    public Rgba32()
    {
    }

    /// <summary>
    /// 根据RGB值创建颜色
    /// </summary>
    /// <param name="red"></param>
    /// <param name="green"></param>
    /// <param name="blue"></param>
    public Rgba32(byte red, byte green, byte blue) : this()
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = byte.MaxValue;
    }

    /// <summary>
    /// 根据RGBA值创建颜色
    /// </summary>
    /// <param name="red"></param>
    /// <param name="green"></param>
    /// <param name="blue"></param>
    /// <param name="alpha"></param>
    public Rgba32(byte red, byte green, byte blue, byte alpha) : this(red, green, blue)
    {
        Alpha = alpha;
    }

    /// <inheritdoc/>
    public static IPixel FromRgb24(in Rgb24 pixel) => new Rgba32(pixel.Red, pixel.Green, pixel.Blue);

    /// <inheritdoc/>
    public static IPixel FromRgb565(in Rgb565 pixel) => new Rgb24(pixel.Red, pixel.Green, pixel.Blue);

    /// <inheritdoc/>
    public static IPixel FromRgba32(in Rgba32 pixel) => new Rgba32(pixel.Red, pixel.Green, pixel.Blue, pixel.Alpha);

    /// <inheritdoc/>
    public void ToRgb24(out Rgb24 pixel) => pixel = new(Red, Green, Blue);

    /// <inheritdoc/>
    public void ToRgb565(out Rgb565 pixel) => pixel = Rgb565.FromGameColor(this.ToGameColor());

    /// <inheritdoc/>
    public void ToRgba32(out Rgba32 pixel) => pixel = new(Red, Green, Blue, Alpha);

    /// <inheritdoc />
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";
}
