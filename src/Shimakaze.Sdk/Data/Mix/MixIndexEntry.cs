using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Data.Mix;

/// <summary>
/// Structure that contains the information of a mix index entry.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct MixIndexEntry
{
    /// <summary>
    /// File ID
    /// </summary>
    /// <remarks>
    /// This is the file ID of the file. <br/>
    /// It may be is CRC32 hash of the file name.
    /// </remarks>
    [FieldOffset(0)]
    public uint Id;
    /// <summary>
    /// File Offset
    /// </summary>
    /// <remarks>
    /// This is the offset of the file in the archive.
    /// </remarks>
    [FieldOffset(sizeof(uint))]
    public int Offset;
    /// <summary>
    /// File Size
    /// </summary>
    /// <remarks>
    /// This is the size of the file in the archive.
    /// </remarks>
    [FieldOffset(sizeof(uint) + sizeof(int))]
    public int Size;
}
