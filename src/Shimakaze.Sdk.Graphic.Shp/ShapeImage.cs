namespace Shimakaze.Sdk.Graphic.Shp;
internal sealed class ShapeImage(ShapeImageFrame[] frames) : IImage
{
    public IImageFrame this[int index] => frames[index];

    public IImageFrame[] Frames => frames;

    public IImageFrame RootFrame => frames[0];
}
