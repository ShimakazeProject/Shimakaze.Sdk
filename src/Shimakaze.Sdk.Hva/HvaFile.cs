namespace Shimakaze.Sdk.Hva;

/// <summary>
/// A hva file describes an animation for a vxl file. In the RA2 mix files a vxl is always paired
/// with a hva file of the same name (so hmec.vxl has hmec.hva).
/// </summary>
/// <remarks>
/// The Header.fileName seems to be the first part of a path name, probably something that was put
/// in there by the conversion tool Westwood used. It seems to be safe to ignore completely.
///
/// The sectionNames correspond to the names in the VXL file. You should really use the names
/// associate a transformation matrix with a section in VXL, but all the vxl/hva pairs I've looked
/// at have the sections in the same order. So you can probably get away with just assuming that the
/// second section in the HVA corresponds to the second section in the VXL etc.
///
/// The HVA format is very simple, just note that the matrices are stored in section-fastest order.
/// </remarks>
public record class HvaFile
{
    internal HvaHeader InternalHeader;

    /// <summary>
    /// </summary>
    public HvaHeader Header
    {
        get => InternalHeader;
        set => InternalHeader = value;
    }
    /// <summary>
    /// The names of all the sections (null-terminated)
    /// </summary>
    public HvaSectionName[] SectionNames { get; set; } = [];
    /// <summary>
    /// </summary>
    public HvaFrame[] Frames { get; set; } = [];
}