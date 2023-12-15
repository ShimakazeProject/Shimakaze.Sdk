using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// 适用于 Voxel 的 Size
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public record struct VoxelSize
{
    /// <summary>
    /// X
    /// </summary>
    public byte X;
    /// <summary>
    /// Y
    /// </summary>
    public byte Y;
    /// <summary>
    /// Z
    /// </summary>
    public byte Z;
}