namespace Shimakaze.Sdk.Tmp;

/// <summary>
/// Flags
/// </summary>
[Flags]
public enum TemplateTileCellFlags : uint
{
    /// <summary>
    /// Has extra data
    /// </summary>
    HasExtraData = 0x00000001,
    /// <summary>
    /// Has Z data (?Always true?)
    /// </summary>
    HasZData = 0x00000002,
    /// <summary>
    /// Has damaged data (?What does this mean?)
    /// </summary>
    HasDamagedData = 0x00000004,
    /// <summary>
    /// other bits always seem to be 0xCDCDCDC8
    /// </summary>
    Unknown = 0xCDCDCDC8,
}
