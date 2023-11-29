namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbBody
/// </summary>
public record struct SectionData
{
    /// <summary>
    /// SpanStart
    /// </summary>
    public int[] SpanStart;
    /// <summary>
    /// SpanEnd
    /// </summary>
    public int[] SpanEnd;
    /// <summary>
    /// Data
    /// </summary>
    public VoxelSpan[] Voxel;
}