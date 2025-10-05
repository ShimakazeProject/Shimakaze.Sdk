using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;
/// <summary>
/// VPL文件
/// </summary>
public sealed record class VoxelPalette(VoxelPaletteHeader Header, Palette Palette, Memory<VoxelPaletteSection> Sections)
{
    /// <summary>
    /// VPL文件头
    /// </summary>
    public VoxelPaletteHeader Header { get; set; } = Header;

    /// <summary>
    /// VPL色板
    /// </summary>
    public Palette Palette { get; set; } = Palette;

    /// <summary>
    /// VPL节
    /// </summary>
    public Memory<VoxelPaletteSection> Sections { get; set; } = Sections;

    /// <summary>
    /// 获取其中一个节
    /// </summary>
    /// <param name="index"> 节索引 </param>
    /// <returns> </returns>
    public VoxelPaletteSection this[int index]
    {
        get => Sections.Span[index];
        set => Sections.Span[index] = value;
    }

    /// <summary>
    /// 体素文件调色板
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static VoxelPalette ReadFrom(Stream stream)
    {
        stream.Read(out VoxelPaletteHeader header);
        Palette palette = Palette.ReadFrom(stream);
        Memory<VoxelPaletteSection> sections = new VoxelPaletteSection[header.SectionCount];
        stream.Read(sections);
        return new(header, palette, sections);
    }

    /// <summary>
    /// 写入体素调色板
    /// </summary>
    /// <param name="stream">流</param>
    public void WriteTo(Stream stream)
    {
        stream.Write(Header);
        Palette.WriteTo(stream);
        stream.Write(Sections);
    }
}
