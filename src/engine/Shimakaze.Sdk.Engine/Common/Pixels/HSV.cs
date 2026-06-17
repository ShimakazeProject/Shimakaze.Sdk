namespace Shimakaze.Sdk.Engine.Common.Pixels;

internal interface IFromHSV<out TPixel> where TPixel : unmanaged, IFromHSV<TPixel>
{
#if NET7_0_OR_GREATER
    static abstract
#endif
    TPixel FromHSV(in HSV hsv);
}

/// <param name="H">色相 (0 - 360)</param>
/// <param name="S">饱和度 (0 - 1)</param>
/// <param name="V">明度 (0 - 1)</param>
internal readonly record struct HSV(float H, float S, float V)
{
    public TPixel To<TPixel>()
        where TPixel : unmanaged, IFromHSV<TPixel>
    {
#if NET7_0_OR_GREATER
        return TPixel.FromHSV(this);
#else
        TPixel pixel = default;
        return pixel.FromHSV(this);
#endif
    }
}
