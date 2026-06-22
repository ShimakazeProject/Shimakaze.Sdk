namespace Shimakaze.Sdk.Engine.Common.Pixels;

internal readonly record struct RGBA32(byte R, byte G, byte B, byte A) : IRGB, IFromHSV<RGBA32>
{
    internal static readonly RGBA32 Transparent = new(0, 0, 0, 0);

    public RGBA32(byte r, byte g, byte b) : this(r, g, b, byte.MaxValue)
    {
    }

#if !NET7_0_OR_GREATER
    RGBA32 IFromHSV<RGBA32>.FromHSV(in HSV hsv) => FromHSV(hsv);
#endif
    public static RGBA32 FromHSV(in HSV hsv)
    {
        float c = hsv.V * hsv.S;             // 色度
        float x = c * (1 - Math.Abs((hsv.H / 60 % 2) - 1));
        float m = hsv.V - c;             // 亮度偏移量

        float r = 0, g = 0, b = 0;

        if (hsv.H is >= 0 and < 60) { r = c; g = x; }
        else if (hsv.H < 120) { r = x; g = c; }
        else if (hsv.H < 180) { g = c; b = x; }
        else if (hsv.H < 240) { g = x; b = c; }
        else if (hsv.H < 300) { r = x; b = c; }
        else if (hsv.H < 360) { r = c; b = x; }

        byte red = (byte)Math.Round((r + m) * 255);
        byte green = (byte)Math.Round((g + m) * 255);
        byte blue = (byte)Math.Round((b + m) * 255);

        return new(red, green, blue);
    }

    public static explicit operator RGBA32(BGRA32 c) => new(c.R, c.G, c.B, c.A);
}
