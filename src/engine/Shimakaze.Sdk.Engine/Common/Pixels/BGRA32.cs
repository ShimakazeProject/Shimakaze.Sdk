using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Common.Pixels;
internal readonly record struct BGRA32(byte B, byte G, byte R, byte A) : IRGB, IFromHSV<BGRA32>
{
    internal static readonly BGRA32 Transparent = new(0, 0, 0, 0);

    public BGRA32(byte b, byte g, byte r) : this(b, g, r, byte.MaxValue)
    {
    }

    public BGRA32(uint value) : this((byte)((value & 0xFF000000) >> 24), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF))
    {
    }

#if !NET7_0_OR_GREATER
    BGRA32 IFromHSV<BGRA32>.FromHSV(in HSV hsv) => FromHSV(hsv);
#endif

    public static BGRA32 FromHSV(in HSV hsv)
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

        return new(blue, green, red);
    }

    public static implicit operator BGRA32(PaletteColor color) => new(color.ExpandedB, color.ExpandedG, color.ExpandedR);
}
