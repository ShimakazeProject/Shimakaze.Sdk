using System.Numerics;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// Transform
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12 * sizeof(float))]
public record struct Transform
{
    /// <summary>
    /// X
    /// </summary>
    public Vector3 X;
    /// <summary>
    /// Y
    /// </summary>
    public Vector3 Y;
    /// <summary>
    /// Z
    /// </summary>
    public Vector3 Z;
    /// <summary>
    /// W
    /// </summary>
    public Vector3 W;
}
