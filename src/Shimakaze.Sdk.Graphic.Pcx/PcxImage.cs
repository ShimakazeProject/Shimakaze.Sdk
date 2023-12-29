using Shimakaze.Sdk.Graphic.Pal;
using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Pcx;

/// <summary>
/// 表示一个PCX图像
/// </summary>
/// <remarks>
/// PCX是单帧图像，只有一个帧
/// </remarks>
public sealed class PcxImage : IImage, IImageFrame
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
        Pixels = new Rgb24[Width * Height];
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
    public Rgb24[] Pixels { get; }

    /// <inheritdoc/>
    public void WriteTo<TPixel>(Stream stream) where TPixel : unmanaged, IPixel
    {
        if (typeof(TPixel) == typeof(Rgb24))
        {
            foreach (Rgb24 pixel in Pixels)
                stream.Write(pixel);
        }
        else if (typeof(TPixel) == typeof(Rgb565))
        {
            foreach (Rgb24 pixel in Pixels)
            {
                pixel.ToRgb565(out var target);
                stream.Write(target);
            }
        }
        else if (typeof(TPixel) == typeof(Rgba32))
        {
            foreach (Rgb24 pixel in Pixels)
            {
                pixel.ToRgba32(out var target);
                stream.Write(target);
            }
        }
    }

    IImageFrame IImage.this[int index] => ((IImage)this).Frames[index];

    IImageFrame[] IImage.Frames => [this];

    IImageFrame IImage.RootFrame => ((IImage)this).Frames[0];
}
