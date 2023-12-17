namespace Shimakaze.Sdk.Graphic.Pixel;

/// <summary>
/// 像素格式实用工具
/// </summary>
public static class IPixelExtensions
{
    /// <summary>
    /// 转换为游戏内实际使用的颜色
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static Rgb24 ToGameColor(this in Rgb565 pixel)
    {
        pixel.ToRgb24(out var result);
        return result;
    }

    /// <inheritdoc cref="ToGameColor(in Rgb565)"/>
    public static Rgb24 ToGameColor(this in Rgb24 pixel) => new(
        unchecked((byte)((pixel.Red & 0b00111110) << 2)),
        unchecked((byte)(pixel.Green << 2)),
        unchecked((byte)((pixel.Blue & 0b00111110) << 2)));

    /// <inheritdoc cref="ToGameColor(in Rgb565)"/>
    public static Rgb24 ToGameColor(this in Rgba32 pixel) => new(
        unchecked((byte)((pixel.Red & 0b00111110) << 2)),
        unchecked((byte)(pixel.Green << 2)),
        unchecked((byte)((pixel.Blue & 0b00111110) << 2)));
}
