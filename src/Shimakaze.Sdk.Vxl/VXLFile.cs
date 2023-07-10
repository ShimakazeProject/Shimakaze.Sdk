using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VXL 文件
/// </summary>
public record struct VXLFile
{
    /// <summary>
    /// 文件头
    /// </summary>
    public VXLHeader Header;
    /// <summary>
    /// 文件色板
    /// </summary>
    public Palette Palette;
    /// <summary>
    /// </summary>
    public SectionHeader[] SectionHeaders;
    /// <summary>
    /// </summary>
    public SectionData[] SectionData;
    /// <summary>
    /// </summary>
    public SectionTailer[] SectionTailers;
}