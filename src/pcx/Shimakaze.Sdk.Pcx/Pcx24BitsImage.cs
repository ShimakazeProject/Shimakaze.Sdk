using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// 24 位色 PCX 图片
/// </summary>
/// <param name="metadata"></param>
public sealed class Pcx24BitsImage(PcxHeader metadata) : PcxImage(metadata)
{
    /// <summary>
    /// 像素数据
    /// </summary>
    public Memory<PaletteColor> Pixels { get; } = new PaletteColor[metadata.Width * metadata.Height];

    /// <inheritdoc/>
    public override IEnumerable<PaletteColor> GetPixels()
    {
        for (int i = 0; i < Pixels.Length; i++)
            yield return Pixels.Span[i];
    }
}
