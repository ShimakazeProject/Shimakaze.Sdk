namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// 片结构
/// </summary>
public record struct VoxelSpan
{
    /// <summary>
    /// Variable number of span segments to make up a whole span
    /// </summary>
    public VoxelSpanSegment[] Sections = Array.Empty<VoxelSpanSegment>();

    /// <summary>
    /// VoxelSpan
    /// </summary>
    public VoxelSpan()
    {
    }
}