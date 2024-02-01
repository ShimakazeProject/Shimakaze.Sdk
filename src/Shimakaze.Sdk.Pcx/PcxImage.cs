using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// 表示一个PCX图像
/// </summary>
/// <remarks>
/// PCX是单帧图像，只有一个帧
/// </remarks>
public sealed class PcxImage
{
    /// <summary>
    /// PCX图像构造器
    /// </summary>
    /// <param name="metadata">元数据</param>
    public PcxImage(PcxHeader metadata)
    {
        Metadata = metadata;
        Width = metadata.WindowXMax - metadata.WindowXMin + 1;
        Height = metadata.WindowYMax - metadata.WindowYMin + 1;
        BitsPerPixel = metadata.ColorPlanes * metadata.BitsPerPlane;
        Pixels = new PaletteColor[Width * Height];
    }

    /// <summary>
    /// 色板
    /// </summary>
    public Palette? Palette { get; internal set; }

    /// <summary>
    /// 图像元数据
    /// </summary>
    public PcxHeader Metadata { get; }

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// 位每像素（颜色深度/颜色位数）
    /// </summary>
    public int BitsPerPixel { get; private set; }

    /// <summary>
    /// 直接获取像素数据
    /// </summary>
    public PaletteColor[] Pixels { get; }

    /// <summary>
    /// 写入RGB24数据到流
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
    {
        foreach (PaletteColor pixel in Pixels)
            stream.Write(pixel);
    }
}
