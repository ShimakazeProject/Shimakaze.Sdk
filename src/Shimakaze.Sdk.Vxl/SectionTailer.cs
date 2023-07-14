using System.Numerics;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// LimbTailer
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 3 * sizeof(int) + 19 * sizeof(float) + 4 * sizeof(byte))]
public record struct SectionTailer
{
    /// <summary>
    /// Offset from the end of headers to the SectionData.spanStart array
    /// </summary>
    [FieldOffset(0 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public uint SpanStartOffset;

    /// <summary>
    /// Offset from the end of headers to the SectionData.spanEnd array
    /// </summary>
    [FieldOffset(1 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public uint SpanEndOffset;

    /// <summary>
    /// Offset from the end of headers to the SectionData.voxelSpan data
    /// </summary>
    [FieldOffset(2 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public uint SpanDataOffset;

    /// <summary>
    /// The scaling factor (always seems to be 0.083)
    /// </summary>
    [FieldOffset(3 * sizeof(int) + 0 * sizeof(float) + 0 * sizeof(byte))]
    public float Scale;

    /// <summary>
    /// The default transformation matrix for the section
    /// </summary>
    [FieldOffset(3 * sizeof(int) + 1 * sizeof(float) + 0 * sizeof(byte))]
    public (Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) Transform;

    /// <summary>
    /// The bounding box of the section
    /// </summary>
    [FieldOffset(3 * sizeof(int) + 13 * sizeof(float) + 0 * sizeof(byte))]
    public (Vector3 Min, Vector3 Max) Bounds;

    /// <summary>
    /// The dimensions of the voxel object
    /// </summary>
    [FieldOffset(3 * sizeof(int) + (16 + 3) * sizeof(float) + 0 * sizeof(byte))]
    public (byte X, byte Y, byte Z) Size;

    /// <summary>
    /// The type of normals (4 == Red Alert 2 normals)
    /// </summary>
    [FieldOffset(3 * sizeof(int) + (16 + 3) * sizeof(float) + 3 * sizeof(byte))]
    public byte NormalType;
}