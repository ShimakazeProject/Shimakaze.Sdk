using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbHeader
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16 + 3 * sizeof(int))]
public record struct LimbHeader
{
    /// <summary>
    /// Limb 的名字
    /// </summary>
    [field: FieldOffset(0)]
    public Int128 Name { get; set; }

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
}
