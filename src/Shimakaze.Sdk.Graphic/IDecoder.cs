namespace Shimakaze.Sdk.Graphic;

/// <summary>
/// 解码器
/// </summary>
public interface IDecoder
{
    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="input">输入流</param>
    /// <returns></returns>
    IImage Decode(Stream input);
}

/// <summary>
/// 解码器
/// </summary>
public interface IDecoder<out TImage> : IDecoder
    where TImage : IImage
{
    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="input">输入流</param>
    /// <returns></returns>
    new TImage Decode(Stream input);
}
