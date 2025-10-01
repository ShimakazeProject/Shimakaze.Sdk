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
public record class HvaFile(HvaHeader Header, Memory<HvaSectionName> SectionNames, HvaFrame[] Frames)
{
    /// <summary>
    /// </summary>
    public HvaHeader Header { get; set; } = Header;

    /// <summary>
    /// The names of all the sections (null-terminated)
    /// </summary>
    public Memory<HvaSectionName> SectionNames { get; set; } = SectionNames;

    /// <summary>
    /// </summary>
    public HvaFrame[] Frames { get; set; } = Frames;

    /// <summary>
    /// Reads a HVA file from a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static HvaFile ReadFrom(Stream stream)
    {
        stream.Read(out HvaHeader header);
        Memory<HvaSectionName> sectionNames = new HvaSectionName[header.NumSections];
        stream.Read(sectionNames);
        var frames = new HvaFrame[header.NumFrames];
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] ??= new(new HvaMatrix[header.NumSections]);
            stream.Read(frames[i].Matrices);
        }

        return new(header, sectionNames, frames);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public void WriteTo(Stream stream)
    {
        stream.Write(Header);
        stream.Write<HvaSectionName>(SectionNames);


        for (int i = 0; i < Frames.Length; i++)
        {
            HvaFrame item = Frames[i];
            stream.Write<HvaMatrix>(item.Matrices);
        }
    }
}
