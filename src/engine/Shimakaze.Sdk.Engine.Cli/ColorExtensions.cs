using System.Drawing;

namespace Shimakaze.Sdk.Engine.Cli;

internal static class ColorExtensions
{
    /// <summary>
    /// 将 RGB 转换为 HSV
    /// </summary>
    /// <param name="color">System.Drawing.Color 对象</param>
    /// <returns>包含 H(0-360), S(0-1), V(0-1) 的元组</returns>
    public static (float H, float S, float V) RgbToHsv(Color color)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = float.Max(r, float.Max(g, b));
        float min = float.Min(r, float.Min(g, b));
        float delta = max - min;

        float h = 0, s, v = max;

        // 计算饱和度 S
        s = (max == 0) ? 0 : delta / max;

        // 计算色相 H
        if (delta < 1e-6) // 灰色或黑色
            h = 0;
        else if (float.Abs(max - r) < 1e-6)
            h = 60 * (((g - b) / delta) % 6);
        else if (float.Abs(max - g) < 1e-6)
            h = 60 * (((b - r) / delta) + 2);
        else if (float.Abs(max - b) < 1e-6)
            h = 60 * (((r - g) / delta) + 4);

        // 确保 H 在 0-360 范围内
        if (h < 0) h += 360;

        return (h, s, v);
    }

    /// <summary>
    /// 将 HSV 转换为 RGB
    /// </summary>
    /// <param name="h">色相 (0 - 360)</param>
    /// <param name="s">饱和度 (0 - 1)</param>
    /// <param name="v">明度 (0 - 1)</param>
    /// <returns>System.Drawing.Color 对象</returns>
    public static Color HsvToRgb(float h, float s, float v)
    {
        float c = v * s;             // 色度
        float x = c * (1 - float.Abs((h / 60) % 2 - 1));
        float m = v - c;             // 亮度偏移量

        float r = 0, g = 0, b = 0;

        if (h >= 0 && h < 60) { r = c; g = x; }
        else if (h < 120) { r = x; g = c; }
        else if (h < 180) { g = c; b = x; }
        else if (h < 240) { g = x; b = c; }
        else if (h < 300) { r = x; b = c; }
        else if (h < 360) { r = c; b = x; }

        byte red = (byte)float.Round((r + m) * 255);
        byte green = (byte)float.Round((g + m) * 255);
        byte blue = (byte)float.Round((b + m) * 255);

        return Color.FromArgb(red, green, blue);
    }

    extension(Color color)
    {
        public (float H, float S, float V) ToHsv() => RgbToHsv(color);
        public static Color FromHsv(float h, float s, float v) => HsvToRgb(h, s, v);

        public Color WithH(float h)
        {
            var (_, s, v) = color.ToHsv();
            return FromHsv(h, s, v);
        }
    }
}
