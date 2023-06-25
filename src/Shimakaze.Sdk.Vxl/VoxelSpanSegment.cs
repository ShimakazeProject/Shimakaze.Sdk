namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelSpanSegment
/// </summary>
public record struct VoxelSpanSegment
{
    /// <summary>
    /// Number of empty voxels before this span segment
    /// </summary>
    public byte SkipCount;
    /// <summary>
    /// Number of voxels in this span segment
    /// </summary>
    public byte NumVoxels;

    /// <summary>
    /// The voxels in the span segment
    /// </summary>
    public Voxel[] Voxels;

    /// <summary>
    /// Always equal to <see cref="NumVoxels"/>
    /// </summary>
    public byte NumVoxels2;

}