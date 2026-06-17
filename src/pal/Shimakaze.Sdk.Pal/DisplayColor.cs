using System.Drawing;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 在游戏引擎里实际展示出来的颜色 (每个颜色的RGB值分别左移两位)
/// </summary>
/// <param name="Color">原颜色</param>
public sealed record class DisplayColor(PaletteColor Color)
{
    /// <summary>
    /// 红色
    /// </summary>
    public byte Red => unchecked((byte)(Color.Red << 2));
    /// <summary>
    /// 绿色
    /// </summary>
    public byte Green => unchecked((byte)(Color.Green << 2));
    /// <summary>
    /// 蓝色
    /// </summary>
    public byte Blue => unchecked((byte)(Color.Blue << 2));

    /// <summary>
    /// 转换为 Drawing 颜色
    /// </summary>
    /// <returns></returns>
    public Color ToColor() => System.Drawing.Color.FromArgb(Red, Green, Blue);
}
