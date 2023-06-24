using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// 体素
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2 * sizeof(byte))]
public record struct SpanVoxel
{
    /// <summary>
    /// 颜色索引
    /// </summary>
    [field: FieldOffset(0)]
    public byte Color { get; set; }

    /// <summary>
    /// 法向
    /// </summary>
    [field: FieldOffset(1)]
    public byte Normal { get; set; }
}
