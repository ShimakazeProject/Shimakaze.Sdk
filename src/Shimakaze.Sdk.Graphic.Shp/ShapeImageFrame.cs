using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Shp;

/// <summary>
/// Shape 帧
/// </summary>
/// <param name="metadata">帧元数据</param>
/// <param name="width">帧宽度</param>
/// <param name="height">帧高度</param>
public sealed class ShapeImageFrame(ShapeFrameHeader metadata, int width, int height) : IImageFrame
{
    /// <summary>
    /// SHP帧元数据
    /// </summary>
    public ShapeFrameHeader Metadata { get; } = metadata;

    /// <inheritdoc/>
    public int Width { get; } = width;

    /// <inheritdoc/>
    public int Height { get; } = height;

    /// <summary>
    /// 直接获取像素数据
    /// </summary>
    public Rgb24[] Pixels { get; } = new Rgb24[width * height];

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
}
