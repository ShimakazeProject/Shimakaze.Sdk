using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VXL 文件
/// </summary>
public record class VXLFile
{
    internal VXLHeader InternalHeader;

    /// <summary>
    /// 文件头
    /// </summary>
    public VXLHeader Header
    {
        get => InternalHeader;
        set => InternalHeader = value;
    }
    /// <summary>
    /// 文件色板
    /// </summary>
    public Palette Palette { get; set; } = new();
    /// <summary>
    /// </summary>
    public SectionHeader[] SectionHeaders { get; set; } = [];
    /// <summary>
    /// </summary>
    public SectionData[] SectionData { get; set; } = [];
    /// <summary>
    /// </summary>
    public SectionTailer[] SectionTailers { get; set; } = [];
}