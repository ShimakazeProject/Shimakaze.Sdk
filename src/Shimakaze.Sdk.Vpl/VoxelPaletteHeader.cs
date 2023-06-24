using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VPL文件头
/// </summary>
/// <param name="RemapPlayerColorStart"></param>
/// <param name="RemapPlayerColorEnd"></param>
/// <param name="SectionCount">节数量</param>
/// <param name="Unknown"></param>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = sizeof(uint) * 4)]
public record struct VoxelPaletteHeader(
    [field: FieldOffset(0)] uint RemapPlayerColorStart,
    [field: FieldOffset(4)] uint RemapPlayerColorEnd,
    [field: FieldOffset(8)] uint SectionCount,
    [field: FieldOffset(12)] uint Unknown
);
