namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// 片结构
/// </summary>
public record class VoxelSpan
{
    /// <summary>
    /// Variable number of span segments to make up a whole span
    /// </summary>
    public VoxelSpanSegment[] Sections { get; set; } = [];

    /// <summary>
    /// VoxelSpan
    /// </summary>
    public VoxelSpan()
    {
    }
}