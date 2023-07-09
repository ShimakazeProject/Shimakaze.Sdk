using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl.Editor;

/// <summary>
/// 颜色 扩展
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// 转 ANSI Color 字符串 用于在终端显示颜色
    /// </summary>
    /// <param name="color">颜色</param>
    /// <param name="isBackground">是否是背景色</param>
    /// <returns></returns>
    public static string GetANSIString(this in Color color, bool isBackground = false) => $"\x1B[{(isBackground ? 4 : 3)}8;2;{color.GetR5()};{color.GetG6()};{color.GetB5()}m";

    /// <summary>
    /// 反转颜色
    /// </summary>
    /// <param name="color">颜色</param>
    /// <returns>被反转后的颜色</returns>
    public static Color GetReverse(this in Color color) => new(
        (byte)~color.Red,
        (byte)~color.Green,
        (byte)~color.Blue
    );
}
