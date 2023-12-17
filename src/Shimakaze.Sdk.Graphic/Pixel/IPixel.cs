namespace Shimakaze.Sdk.Graphic.Pixel;

/// <summary>
/// 像素格式
/// </summary>
public interface IPixel
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// 读取为16位色
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    static abstract IPixel FromRgb565(in Rgb565 pixel);
    /// <summary>
    /// 读取为24位色
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    static abstract IPixel FromRgb24(in Rgb24 pixel);
    /// <summary>
    /// 读取为32位色
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    static abstract IPixel FromRgba32(in Rgba32 pixel);
#endif
    /// <summary>
    /// 转换为16位色
    /// </summary>
    /// <param name="pixel"></param>
    void ToRgb565(out Rgb565 pixel);
    /// <summary>
    /// 转换为24位色
    /// </summary>
    /// <param name="pixel"></param>
    void ToRgb24(out Rgb24 pixel);
    /// <summary>
    /// 转换为32位色
    /// </summary>
    /// <param name="pixel"></param>
    void ToRgba32(out Rgba32 pixel);
}
