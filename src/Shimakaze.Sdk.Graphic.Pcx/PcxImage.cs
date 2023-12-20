namespace Shimakaze.Sdk.Graphic.Pcx;
internal sealed class PcxImage(PcxImageFrame frame) : IImage
{
    public IImageFrame this[int index] => Frames[index];

    public IImageFrame[] Frames { get; } = [frame];

    public IImageFrame RootFrame => Frames[0];
}
