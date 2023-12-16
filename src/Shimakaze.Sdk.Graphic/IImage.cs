namespace Shimakaze.Sdk.Graphic;

/// <summary>
/// 表示一个图像
/// </summary>
public interface IImage
{
    /// <summary>
    /// 图片帧
    /// </summary>
    IImageFrame[] Frames { get; }

    /// <summary>
    /// 图片帧
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IImageFrame this[int index] { get; }

    /// <summary>
    /// 图片的第一个帧
    /// </summary>
    IImageFrame RootFrame { get; }
}
