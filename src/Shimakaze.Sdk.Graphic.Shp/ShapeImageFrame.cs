using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Shp;

internal sealed class ShapeImageFrame(int size) : IImageFrame
{
    internal Rgb24[] Pixels { get; } = new Rgb24[size];

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
