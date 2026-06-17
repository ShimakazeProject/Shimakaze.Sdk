using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 颜色
/// </summary>
/// <param name="Red">红色</param>
/// <param name="Green">绿色</param>
/// <param name="Blue">蓝色</param>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = BytePerPixel)]
public readonly record struct PaletteColor(
    [field: FieldOffset(0)] byte Red,
    [field: FieldOffset(1)] byte Green,
    [field: FieldOffset(2)] byte Blue)
{
    /// <summary>
    /// 色彩深度
    /// </summary>
    public const int BitPerPixel = BytePerPixel * 8;

    /// <summary>
    /// 字节每像素
    /// </summary>
    public const int BytePerPixel = sizeof(byte) * 3;

    /// <summary>
    /// 获取当前 <see cref="PaletteColor"/> 对象的 #HEX 值
    /// </summary>
    /// <returns></returns>
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";

    /// <summary>
    /// 创建一个 <see cref="DisplayColor"/> 对象
    /// </summary>
    public DisplayColor AsDisplay() => this;

    /// <summary>
    /// 创建一个 <see cref="PaletteColor"/> 对象
    /// </summary>
    /// <param name="rgb888">0x00RRGGBB</param>
    public static unsafe implicit operator PaletteColor(int rgb888) => *(PaletteColor*)&rgb888;

    /// <summary>
    /// 转换为 24 位 RGB 颜色值
    /// </summary>
    /// <param name="color"></param>
    public static unsafe explicit operator int(PaletteColor color)
    {
        int i = color.Red;
        i <<= 8;
        i |= color.Green;
        i <<= 8;
        i |= color.Blue;
        return i;
    }

    /// <summary>
    /// 创建一个 <see cref="PaletteColor"/> 对象
    /// </summary>
    /// <param name="rgb565"></param>
    public static unsafe implicit operator PaletteColor(short rgb565)
    {
        int rgb888 = 0;
        unchecked
        {
            rgb888 |= (rgb565 & 0b11111000_00000000) >> 11;
            rgb888 <<= 8;
            rgb888 |= (rgb565 & 0b00000111_11100000) >> 5;
            rgb888 <<= 8;
            rgb888 |= (rgb565 & 0b00000000_00011111) >> 0;
        }
        return (PaletteColor)rgb888;
    }

    /// <summary>
    /// 将24位色转换为16位色
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static unsafe explicit operator ushort(PaletteColor pixel)
    {
        /*
         * RRRRRRRR | GGGGGGGG | BBBBBBBB
         *   >>= 1  |          |   >>= 1
         * #RRRRRRR |   <<= 2  | #BBBBBBB
         *   <<= 3  |          |   <<= 3
         * RRRRR### | GGGGGG## | BBBBB###
         * 
         * => RRRRRGGG GGGBBBBB
         */
        int value = 0;
        // ######## ######## ######## ########
        value |= pixel.Red & 0b11111000;
        // ######## ######## ######## RRRRR###
        value <<= 8 - 3;
        // ######## ######## ###RRRRR ########
        value |= pixel.Green & 0b11111100;
        // ######## ######## ###RRRRR GGGGGG##
        value <<= 8 - 2;
        // ######## #####RRR RRGGGGGG ########
        value |= pixel.Blue & 0b11111000;
        // ######## #####RRR RRGGGGGG BBBBB###
        value >>= 3;
        // ######## ######## RRRRRGGG GGGBBBBB
        return unchecked((ushort)value);
    }

    /// <inheritdoc cref="AsDisplay"/>
    /// <param name="color"></param>
    public static implicit operator DisplayColor(PaletteColor color) => new(color);
}
