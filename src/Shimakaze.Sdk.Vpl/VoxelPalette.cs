using System.Collections;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;
/// <summary>
/// VPL文件
/// </summary>
public partial struct VoxelPalette : IEnumerable<VoxelPaletteSection>
{
    /// <summary>
    /// VPL文件头
    /// </summary>
    public VoxelPaletteHeader Header { get; set; }

    /// <summary>
    /// VPL色板
    /// </summary>
    public Palette Palette { get; set; }

    /// <summary>
    /// VPL节
    /// </summary>
    public VoxelPaletteSection[] Sections { get; set; }

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

    /// <inheritdoc/>
    public readonly IEnumerator<VoxelPaletteSection> GetEnumerator() => new Enumerator(this);
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
