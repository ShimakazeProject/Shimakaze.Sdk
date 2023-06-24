using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbHeader
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16 + 3 * sizeof(int))]
public record struct LimbHeader
{
    [FieldOffset(0)]
    private Int128 _name;

    /// <summary>
    /// Number
    /// </summary>
    [field: FieldOffset(16)]
    public int Number { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [field: FieldOffset(16 + sizeof(int))]
    public int Unknown { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [field: FieldOffset(16 + sizeof(int) * 2)]
    public int Unknown2 { get; set; }

    /// <summary>
    /// Limb 的名字
    /// </summary>
    public string Name
    {
        readonly get
        {
            unsafe
            {
                var tmp = _name;
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
                    _name = *(Int128*)tmp;
                }
            }
        }
    }
}
