using System.Numerics;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbTailer
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 3 * sizeof(int) + 19 * sizeof(float) + 4 * sizeof(byte))]
public record struct LimbTailer
{
    /// <summary>
    /// SpanStartOffset
    /// </summary>
    [field: FieldOffset(0 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public int SpanStartOffset { get; set; }
    /// <summary>
    /// SpanEndOffset
    /// </summary>
    [field: FieldOffset(1 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public int SpanEndOffset { get; set; }
    /// <summary>
    /// SpanDataOffset
    /// </summary>
    [field: FieldOffset(2 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public int SpanDataOffset { get; set; }
    /// <summary>
    /// Transform
    /// </summary>
    [field: FieldOffset(3 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public (Vector4 X, Vector4 Y, Vector4 Z, float Scale) Transform { get; set; }
    /// <summary>
    /// Scale
    /// </summary>
    [field: FieldOffset(3 * sizeof(int) + 13 * sizeof(float) + 0 * sizeof(byte))]
    public (Vector3 Min, Vector3 Max) Scale { get; set; }
    /// <summary>
    /// Size
    /// </summary>
    [field: FieldOffset(3 * sizeof(int) + (16 + 3) * sizeof(float) + 0 * sizeof(byte))]
    public (byte X, byte Y, byte Z) Size { get; set; }
    /// <summary>
    /// Always 2
    /// </summary>
    [field: FieldOffset(3 * sizeof(int) + (16 + 3) * sizeof(float) + 3 * sizeof(byte))]
    public byte Unknown { get; set; }
}
