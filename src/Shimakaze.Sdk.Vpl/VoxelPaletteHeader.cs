using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VPL文件头
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = sizeof(uint) * 4)]
public record struct VoxelPaletteHeader
{
    /// <summary>
    /// RemapPlayerColorStart
    /// </summary>
    [FieldOffset(0)]
    public uint RemapPlayerColorStart;
    /// <summary>
    /// RemapPlayerColorEnd
    /// </summary>
    [FieldOffset(4)]
    public uint RemapPlayerColorEnd;
    /// <summary>
    /// 节数量
    /// </summary>
    [FieldOffset(8)]
    public uint SectionCount;
    /// <summary>
    /// Unknown
    /// </summary>
    [FieldOffset(12)]
    public uint Unknown;
}