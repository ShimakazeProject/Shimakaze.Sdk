using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Hva;
/// <summary>
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public record struct HvaHeader
{
    /// <summary>
    /// Garbage, ignore (seems to be some part of a file path)
    /// </summary>
    [FieldOffset(0)]
    public HvaSectionName FileName;

    /// <summary>
    /// The number of frames the animation has
    /// </summary>
    [FieldOffset(16)]
    public uint NumFrames;

    /// <summary>
    /// The number of sections that are animated
    /// </summary>
    [FieldOffset(16 + sizeof(uint))]
    public uint NumSections;
}