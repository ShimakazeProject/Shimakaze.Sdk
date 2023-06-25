using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;
/// <summary>
/// VPL文件
/// </summary>
public record struct VoxelPalette
{
    /// <summary>
    /// VPL文件头
    /// </summary>
    public VoxelPaletteHeader Header;

    /// <summary>
    /// VPL色板
    /// </summary>
    public Palette Palette;

    /// <summary>
    /// VPL节
    /// </summary>
    public VoxelPaletteSection[] Sections;

    /// <summary>
    /// 获取其中一个节
    /// </summary>
    /// <param name="index">节索引</param>
    /// <returns></returns>
    public readonly VoxelPaletteSection this[int index]
    {
        get => Sections[index];
        set => Sections[index] = value;
    }
}