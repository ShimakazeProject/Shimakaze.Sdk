using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;
/// <summary>
/// VXL 头
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16 + sizeof(int) * 4 + sizeof(short))]
public record struct VXLHeader
{
    /// <summary>
    /// 文件头标识 总是 "Voxel Animation\0"
    /// </summary>
    [FieldOffset(0)]
    public Int128 FileType;
    /// <summary>
    /// </summary>
    [FieldOffset(16)]
    public uint Unknown;
    /// <summary>
    /// Limb 的 数量
    /// </summary>
    [FieldOffset(16 + sizeof(int))]
    public uint NumSections;

    /// <summary>
    /// Limb 的 数量
    /// </summary>
    [FieldOffset(16 + sizeof(int) * 2)]
    public uint NumSections2;

    /// <summary>
    /// 主体部分大小
    /// </summary>
    [FieldOffset(16 + sizeof(int) * 3)]
    public uint BodySize;
    /// <summary>
    /// </summary>
    [FieldOffset(16 + sizeof(int) * 4)]
    public byte StartPaletteRemap;
    /// <summary>
    /// </summary>
    [FieldOffset(16 + sizeof(int) * 4 + 1)]
    public byte EndPaletteRemap;
}