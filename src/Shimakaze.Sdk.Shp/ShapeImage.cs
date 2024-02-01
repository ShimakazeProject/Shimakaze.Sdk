namespace Shimakaze.Sdk.Shp;

/// <summary>
/// SHP图像
/// </summary>
/// <param name="metadata">图像元数据</param>
/// <param name="frames">图像帧</param>
public sealed class ShapeImage(ShapeFileHeader metadata, ShapeImageFrame[] frames)
{
    /// <summary>
    /// SHP文件元数据
    /// </summary>
    public ShapeFileHeader Metadata { get; } = metadata;

    /// <summary>
    /// 获取帧
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ShapeImageFrame this[int index] => frames[index];

    /// <summary>
    /// 所有帧
    /// </summary>
    public ShapeImageFrame[] Frames => frames;

    /// <summary>
    /// 第一个帧
    /// </summary>
    public ShapeImageFrame RootFrame => frames[0];
}
