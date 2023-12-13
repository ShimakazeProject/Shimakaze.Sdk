using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Structure that contains the information of a mix index entry.
/// </summary>
/// <param name="Id">
/// File ID <br /> This is the file ID of the file. <br /> It may be is CRC32 hash of the file name.
/// </param>
/// <param name="Offset"> File Offset <br /> This is the offset of the file in the archive. </param>
/// <param name="Size"> File Size <br /> This is the size of the file in the archive. </param>
[StructLayout(LayoutKind.Sequential)]
public record struct MixEntry(uint Id, int Offset, int Size);
