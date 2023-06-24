using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Vxl;
/// <summary>
/// VXL 头
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16 + sizeof(int) * 4 + sizeof(short))]
public record struct VoxelHeader
{
    /// <summary>
    /// 文件头标识 总是 "Voxel Animation\0"
    /// </summary>
    [field: FieldOffset(0)]
    public Int128 FileType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [field: FieldOffset(16)]
    public int Unknown { get; set; }
    /// <summary>
    /// Limb 的 数量
    /// </summary>
    [field: FieldOffset(16 + sizeof(int))]
    public int LimbsCount { get; set; }

    /// <summary>
    /// Limb 的 数量
    /// </summary>
    [field: FieldOffset(16 + sizeof(int) * 2)]
    public int LimbsCount2 { get; set; }

    /// <summary>
    /// 主体部分大小
    /// </summary>
    [field: FieldOffset(16 + sizeof(int) * 3)]
    public int BodySize { get; set; }
    /// <summary>
    /// Always 0x1f10
    /// </summary>
    [field: FieldOffset(16 + sizeof(int) * 4)]
    public short Unknown2 { get; set; }
}
