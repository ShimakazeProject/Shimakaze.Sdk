namespace Shimakaze.Sdk.Graphic;

/// <summary>
/// 解码器
/// </summary>
/// <typeparam name="TImage">图像格式</typeparam>
public abstract class Decoder<TImage> : IDecoder<TImage>
    where TImage : IImage
{
    /// <inheritdoc/>
    public abstract TImage Decode(Stream input);
    IImage IDecoder.Decode(Stream input) => Decode(input);
}