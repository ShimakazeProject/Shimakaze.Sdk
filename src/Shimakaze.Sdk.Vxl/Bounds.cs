using System.Numerics;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// Bounds
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6 * sizeof(float))]
public record struct Bounds
{
    /// <summary>
    /// Min
    /// </summary>
    public Vector3 Min;
    /// <summary>
    /// Max
    /// </summary>
    public Vector3 Max;
}