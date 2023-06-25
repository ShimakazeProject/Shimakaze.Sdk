using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbHeader
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16 + 3 * sizeof(int))]
public record struct SectionHeader
{
    /// <summary>
    /// Limb 的名字
    /// </summary>
    [FieldOffset(0)]
    public Int128 Name;

    /// <summary>
    /// Number
    /// </summary>
    [FieldOffset(16)]
    public uint Number;

    /// <summary>
    /// 
    /// </summary>
    [FieldOffset(16 + sizeof(int))]
    public uint Unknown;

    /// <summary>
    /// 
    /// </summary>
    [FieldOffset(16 + sizeof(int) * 2)]
    public uint Unknown2;
}
