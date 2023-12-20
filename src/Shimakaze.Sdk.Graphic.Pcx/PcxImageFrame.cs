using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Pcx;

internal sealed class PcxImageFrame(int width, int height) : IImageFrame
{
    public int Width { get; } = width;

    public int Height { get; } = height;

    internal Rgb24[] Pixels { get; } = new Rgb24[width * height];

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
