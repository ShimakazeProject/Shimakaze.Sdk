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

    /// <summary>
    /// 从RGB565（16位色）数值中创建颜色
    /// </summary>
    /// <param name="rgb565"></param>
    public PaletteColor(ushort rgb565)
    {
        Red =  unchecked((byte)((rgb565 & 0b11111000_00000000) >> 11));
        Green =  unchecked((byte)((rgb565 & 0b00000111_11100000) >> 5));
        Blue =  unchecked((byte)((rgb565 & 0b00000000_00011111) >> 0));
    }

    /// <inheritdoc />
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";
}
