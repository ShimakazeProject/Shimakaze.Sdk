using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic;

/// <summary>
/// 图片帧
/// </summary>
public interface IImageFrame
{
    /// <summary>
    /// 按特定像素格式写到流
    /// </summary>
    /// <typeparam name="TPixel">像素格式</typeparam>
    /// <param name="stream">流</param>
    /// <exception cref="NotSupportedException">不受支持的像素格式</exception>
    void WriteTo<TPixel>(Stream stream)
        where TPixel : unmanaged, IPixel;
}
