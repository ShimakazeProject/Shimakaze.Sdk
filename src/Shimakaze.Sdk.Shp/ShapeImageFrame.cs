namespace Shimakaze.Sdk.Shp;

/// <summary>
/// Shape 帧
/// </summary>
/// <param name="metadata">帧元数据</param>
/// <param name="width">帧宽度</param>
/// <param name="height">帧高度</param>
public sealed class ShapeImageFrame(ShapeFrameHeader metadata, int width, int height)
{
    /// <summary>
    /// Shape 帧
    /// </summary>
    /// <param name="metadata">帧元数据</param>
    /// <param name="width">帧宽度</param>
    /// <param name="height">帧高度</param>
    /// <param name="data">数据</param>
    public ShapeImageFrame(ShapeFrameHeader metadata, int width, int height, byte[] data) : this(metadata, width, height)
    {
        Indexes = data;
    }
    /// <summary>
    /// SHP帧元数据
    /// </summary>
    public ShapeFrameHeader Metadata { get; } = metadata;

    /// <inheritdoc/>
    public int Width { get; } = width;

    /// <inheritdoc/>
    public int Height { get; } = height;

    /// <summary>
    /// 直接获取像素数据
    /// </summary>
    public byte[] Indexes { get; } = new byte[width * height];

    /// <summary>
    /// 写入索引数据到流
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
    {
        stream.Write(Indexes);
    }
}
