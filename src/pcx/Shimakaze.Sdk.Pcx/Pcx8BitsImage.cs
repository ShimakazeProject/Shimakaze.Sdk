using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// 索引色 PCX 图片
/// </summary>
/// <param name="metadata"></param>
/// <param name="indexes"></param>
/// <param name="palette"></param>
public sealed class Pcx8BitsImage(PcxHeader metadata, Memory<byte> indexes, Palette palette) : PcxImage(metadata)
{
    /// <summary>
    /// 索引
    /// </summary>
    public Memory<byte> Indexes { get; } = indexes;

    /// <summary>
    /// 色板
    /// </summary>
    public Palette Palette { get; } = palette;

    /// <inheritdoc/>
    public override IEnumerable<PaletteColor> GetPixels()
    {
        for (int i = 0; i < Indexes.Length; i++)
            yield return Palette[i];
    }
}
