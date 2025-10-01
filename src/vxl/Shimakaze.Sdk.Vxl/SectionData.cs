namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbBody
/// </summary>
public record class SectionData(Memory<int> SpanStart, Memory<int> SpanEnd, VoxelSpan[] Voxel)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="spanStartCapacity"></param>
    /// <param name="spanEndCapacity"></param>
    /// <param name="voxelCapacity"></param>
    public SectionData(int spanStartCapacity, int spanEndCapacity, int voxelCapacity)
        : this(new int[spanStartCapacity], new int[spanEndCapacity], new VoxelSpan[voxelCapacity])
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="spanCapacity"></param>
    public SectionData(int spanCapacity)
        : this(spanCapacity, spanCapacity, spanCapacity)
    {
    }

    /// <summary>
    /// SpanStart
    /// </summary>
    public Memory<int> SpanStart { get; set; } = SpanStart;
    /// <summary>
    /// SpanEnd
    /// </summary>
    public Memory<int> SpanEnd { get; set; } = SpanEnd;
    /// <summary>
    /// Data
    /// </summary>
    public VoxelSpan[] Voxel { get; set; } = Voxel;
}
