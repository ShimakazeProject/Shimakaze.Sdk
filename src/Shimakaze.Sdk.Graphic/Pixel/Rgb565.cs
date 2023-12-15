using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Graphic.Pixel;

/// <summary>
/// 16位色
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
public readonly record struct Rgb565 : IPixel
{
    /// <inheritdoc cref="Rgba32.Size"/>
    public const int Size = sizeof(ushort);
    /// <summary>
    /// 16位色值
    /// </summary>
    public readonly ushort Value;

    /// <inheritdoc cref="Rgba32()"/>
    public Rgb565()
    {
    }

    /// <summary>
    /// 根据16位色值创建颜色
    /// </summary>
    /// <param name="value"></param>
    public Rgb565(ushort value) : this()
    {
        Value = value;
    }

    /// <inheritdoc cref="Rgba32.Red"/>
    public readonly byte Red => unchecked((byte)((Value & 0b11111000_00000000) >> 11));
    /// <inheritdoc cref="Rgba32.Green"/>
    public readonly byte Green => unchecked((byte)((Value & 0b00000111_11100000) >> 5));
    /// <inheritdoc cref="Rgba32.Blue"/>
    public readonly byte Blue => unchecked((byte)((Value & 0b00000000_00011111) >> 0));

    /// <summary>
    /// 从游戏颜色创建
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static Rgb565 FromGameColor(in Rgb24 pixel)
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
        return new Rgb565(unchecked((ushort)value));
    }

    /// <inheritdoc/>
    public static IPixel FromRgb24(in Rgb24 pixel) => FromGameColor(pixel.ToGameColor());

    /// <inheritdoc/>
    public static IPixel FromRgb565(in Rgb565 pixel) => new Rgb565(pixel.Value);

    /// <inheritdoc/>
    public static IPixel FromRgba32(in Rgba32 pixel) => FromGameColor(pixel.ToGameColor());

    /// <inheritdoc/>
    public void ToRgb24(out Rgb24 pixel) => pixel = new(Red, Green, Blue);

    /// <inheritdoc/>
    public void ToRgb565(out Rgb565 pixel) => pixel = new(Value);

    /// <inheritdoc/>
    public void ToRgba32(out Rgba32 pixel) => pixel = new(Red, Green, Blue);

    /// <inheritdoc />
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";
}
