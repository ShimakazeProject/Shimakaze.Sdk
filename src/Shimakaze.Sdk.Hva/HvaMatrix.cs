using System.Numerics;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Hva;

/// <summary>
/// 
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public record struct HvaMatrix
{
    /// <summary>
    /// The transformation matrix
    /// </summary>
    [FieldOffset(0 * 3 * sizeof(float))]
    public Vector3 M1;
    /// <summary>
    /// The transformation matrix
    /// </summary>
    [FieldOffset(1 * 3 * sizeof(float))]
    public Vector3 M2;
    /// <summary>
    /// The transformation matrix
    /// </summary>
    [FieldOffset(2 * 3 * sizeof(float))]
    public Vector3 M3;
    /// <summary>
    /// The transformation matrix
    /// </summary>
    [FieldOffset(3 * 3 * sizeof(float))]
    public Vector3 M4;
}