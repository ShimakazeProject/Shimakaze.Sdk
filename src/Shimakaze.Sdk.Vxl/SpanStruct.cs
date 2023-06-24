namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// 片结构
/// </summary>
public record struct SpanStruct
{
    /// <summary>
    /// 
    /// </summary>
    public byte SkipCount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public byte NumVoxels { get; set; }
    /// <summary>
    /// 一般与 <see cref="NumVoxels"/> 相同
    /// </summary>
    public byte NumVoxels2 { get; set; }

    /// <summary>
    /// 体素
    /// </summary>
    public SpanVoxel[] Voxels { get; set; }
}
