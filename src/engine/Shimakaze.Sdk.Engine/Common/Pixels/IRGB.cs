namespace Shimakaze.Sdk.Engine.Common.Pixels;

internal interface IRGB
{
    byte R { get; }
    byte G { get; }
    byte B { get; }
}

internal static class RGBExtensions
{
    extension<TRGB>(TRGB rgb)
        where TRGB : unmanaged, IRGB
    {
        /// <summary>
        /// 将 RGB 转换为 HSV
        /// </summary>
        /// <returns>包含 H(0-360), S(0-1), V(0-1) 的元组</returns>
        public HSV ToHSV()
        {
            float r = rgb.R / 255f;
            float g = rgb.G / 255f;
            float b = rgb.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0, s, v = max;

            // 计算饱和度 S
            s = (max == 0) ? 0 : delta / max;

            // 计算色相 H
            if (delta < 1e-6) // 灰色或黑色
                h = 0;
            else if (Math.Abs(max - r) < 1e-6)
                h = 60 * ((g - b) / delta % 6);
            else if (Math.Abs(max - g) < 1e-6)
                h = 60 * (((b - r) / delta) + 2);
            else if (Math.Abs(max - b) < 1e-6)
                h = 60 * (((r - g) / delta) + 4);

            // 确保 H 在 0-360 范围内
            if (h < 0) h += 360;

            return new(h, s, v);
        }
    }
}
