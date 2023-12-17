namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelSpanSegment
/// </summary>
public record class VoxelSpanSegment
{
    /// <summary>
    /// Number of empty voxels before this span segment
    /// </summary>
    public byte SkipCount { get; set; }
    /// <summary>
    /// Number of voxels in this span segment
    /// </summary>
    public byte NumVoxels { get; set; }

    /// <summary>
    /// The voxels in the span segment
    /// </summary>
    public Voxel[] Voxels { get; set; } = [];

    /// <summary>
    /// Always equal to <see cref="NumVoxels" />
    /// </summary>
    public byte NumVoxels2 { get; set; }
}