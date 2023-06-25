using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// 体素
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2 * sizeof(byte))]
public record struct Voxel
{
    /// <summary>
    /// The colour of the voxel (to be looked up in a palette)
    /// </summary>
    [FieldOffset(0)]
    public byte Color;

    /// <summary>
    /// The normal vector of the voxel
    /// </summary>
    [FieldOffset(1)]
    public byte Normal;
}
