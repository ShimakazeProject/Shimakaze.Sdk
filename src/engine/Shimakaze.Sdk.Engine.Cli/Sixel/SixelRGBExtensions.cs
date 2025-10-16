using System.Diagnostics;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Cli.Sixel;

/// <summary>
/// Sixel RGB 颜色扩展
/// </summary>
public static class SixelRGBExtensions
{
    /// <inheritdoc cref="SixelWriter.RegistColor(byte, SixelColorType, int, int, int)"/>
    internal static SixelWriter RegistColor(this SixelWriter sixel, int index, DisplayColor color)
    {
        return sixel.RegistColor(
            (byte)index,
            color.Red,
            color.Green,
            color.Blue);
    }

    /// <inheritdoc cref="SixelWriter.RegistColor(byte, SixelColorType, int, int, int)"/>
    public static SixelWriter RegistColor(this SixelWriter sixel, byte index, int r, int g, int b)
    {
        r = r * 100 / 255;
        g = g * 100 / 255;
        b = b * 100 / 255;

        return sixel.RegistColor(index, SixelColorType.RGB, r, g, b);
    }

    /// <inheritdoc cref="SixelWriter.RegistColor(byte, SixelColorType, int, int, int)"/>
    public static SixelWriter RegistColor(this SixelWriter sixel, byte index, int rgb)
    {
        int r = (rgb & 0xFF0000) >> 0x10;
        int g = (rgb & 0x00FF00) >> 0x08;
        int b = (rgb & 0x0000FF) >> 0x00;

        return sixel.RegistColor(index, r, g, b);
    }

    /// <inheritdoc cref="SixelWriter.RegistColor(byte, SixelColorType, int, int, int)"/>
    public static SixelWriter RegistColor(this SixelWriter sixel, byte index, float r, float g, float b)
    {
        Debug.Assert(r is >= 0 and <= 1);
        Debug.Assert(g is >= 0 and <= 1);
        Debug.Assert(b is >= 0 and <= 1);

        return sixel.RegistColor(index, SixelColorType.RGB, (int)Math.Round(r * 100), (int)Math.Round(g * 100), (int)Math.Round(b * 100));
    }
}
