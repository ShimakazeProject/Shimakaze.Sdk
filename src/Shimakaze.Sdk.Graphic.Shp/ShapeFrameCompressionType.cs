namespace Shimakaze.Sdk.Graphic.Shp;

/// <summary>
/// SHP压缩方式
/// </summary>
[Flags]
public enum ShapeFrameCompressionType : byte
{
    /// <summary>
    /// 未压缩
    /// </summary>
    UnCompression = 1,
    /// <summary>
    /// Scanline
    /// </summary>
    Scanline = 2,
    /// <summary>
    /// RLE压缩的Scanline
    /// </summary>
    ScanlineRLE = 3,
}
