namespace Shimakaze.Sdk.Graphic.Shp;

/// <summary>
/// SHP压缩方式
/// </summary>
[Flags]
public enum ShapeFrameCompressionType : byte
{
    /// <summary>
    /// RLE压缩
    /// </summary>
    /// <remarks>
    /// Westwood Studio使用了一种特殊的压缩方式。<br/>
    /// 一些用于单位的SHP内部使用这种压缩方式。<br/>
    /// 这种类型的SHP内部通常会存在大量的0，而 Westwood Studio
    /// 巧妙的使用这种方式压缩了大量的数据。<br/>
    /// 00 ## 其中##表示有多少个0。
    /// </remarks>
    ScanlineRLE = 2,
}
