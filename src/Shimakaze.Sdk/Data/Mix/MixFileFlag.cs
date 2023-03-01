namespace Shimakaze.Sdk.Data.Mix;

/// <summary>
/// Flags for MixFile.
/// </summary>
[Flags]
public enum MixFileFlag : uint
{
    /// <summary>
    /// 0x00_00_00_00
    /// </summary>
    /// <remarks>
    /// It is a normal file.
    /// </remarks>
    NONE = 0,
    /// <summary>
    /// 0x00_01_00_00
    /// </summary>
    /// <remarks>
    /// This file has a CheckSum.
    /// </remarks>
    CHECKSUM = 0x00010000,
    /// <summary>
    /// 0x00_02_00_00
    /// </summary>
    /// <remarks>
    /// This file is be encrypted.
    /// </remarks>
    ENCRYPTED = 0x00020000,
}
