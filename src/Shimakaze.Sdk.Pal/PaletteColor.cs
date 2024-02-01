using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 24位色
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
public readonly record struct PaletteColor
{
    /// <summary>
    /// 字节每像素（色彩深度）
    /// </summary>
    public const int Size = sizeof(byte) * 3;
    /// <summary>
    /// 红色
    /// </summary>
    public readonly byte Red;
    /// <summary>
    /// 绿色
    /// </summary>
    public readonly byte Green;
    /// <summary>
    /// 蓝色
    /// </summary>
    public readonly byte Blue;

    /// <summary>
    /// 创建一个颜色
    /// </summary>
    public PaletteColor()
    {
    }

    /// <summary>
    /// 根据RGB值创建颜色
    /// </summary>
    /// <param name="red">红色</param>
    /// <param name="green">绿色</param>
    /// <param name="blue">蓝色</param>
    public PaletteColor(byte red, byte green, byte blue) : this()
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    /// <inheritdoc />
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";
}
