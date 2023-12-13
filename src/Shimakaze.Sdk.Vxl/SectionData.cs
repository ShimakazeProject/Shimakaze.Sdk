namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbBody
/// </summary>
public record class SectionData
{
    /// <summary>
    /// SpanStart
    /// </summary>
    public int[] SpanStart { get; set; } = [];
    /// <summary>
    /// SpanEnd
    /// </summary>
    public int[] SpanEnd { get; set; } = [];
    /// <summary>
    /// Data
    /// </summary>
    public VoxelSpan[] Voxel { get; set; } = [];
}