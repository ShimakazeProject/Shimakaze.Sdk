using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Vxl;
/// <summary>
/// VXL 头
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16 + sizeof(int) * 4 + sizeof(short))]
public record struct VoxelHeader
{
    [FieldOffset(0)]
    private Int128 _fileType;
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

    /// <summary>
    /// 文件头标识 总是 "Voxel Animation\0"
    /// </summary>
    public string FileType
    {
        readonly get
        {
            unsafe
            {
                var tmp = _fileType;
                return new string((sbyte*)&tmp, 0, 16).Split('\0').First();
            }
        }
        set
        {
            unsafe
            {
                byte* tmp = stackalloc byte[16];
                char[] chars = value.ToCharArray();
                fixed (char* pChar = chars)
                {
                    int data = Encoding.ASCII.GetBytes(pChar, chars.Length, tmp, 16);
                    _fileType = *(Int128*)tmp;
                }
            }
        }
    }
}
