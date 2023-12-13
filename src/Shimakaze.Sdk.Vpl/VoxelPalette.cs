using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;
/// <summary>
/// VPL文件
/// </summary>
public record class VoxelPalette
{
    internal VoxelPaletteHeader InternalHeader;

    /// <summary>
    /// VPL文件头
    /// </summary>
    public VoxelPaletteHeader Header
    {
        get => InternalHeader;
        set => InternalHeader = value;
    }

    /// <summary>
    /// VPL色板
    /// </summary>
    public Palette Palette { get; set; } = new();

    /// <summary>
    /// VPL节
    /// </summary>
    public VoxelPaletteSection[] Sections { get; set; } = [];

    /// <summary>
    /// 获取其中一个节
    /// </summary>
    /// <param name="index"> 节索引 </param>
    /// <returns> </returns>
    public VoxelPaletteSection this[int index]
    {
        get => Sections[index];
        set => Sections[index] = value;
    }
}