using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Data.Mix;

/// <summary>
/// Mix File Header
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct MixFileInfo
{
    /// <summary>
    /// File Flag
    /// </summary>
    [FieldOffset(0)]
    public uint Flag;

    /// <summary>
    /// File Count
    /// </summary>
    /// <remarks>
    /// it is the number of files in the archive.
    /// </remarks>
    [FieldOffset(sizeof(uint))]
    public short Files;

    /// <summary>
    /// File Size
    /// </summary>
    /// <remarks>
    /// it is the sum of all file sizes
    /// </remarks>
    [FieldOffset(sizeof(MixFileFlag) + sizeof(short))]
    public int Size;
}
