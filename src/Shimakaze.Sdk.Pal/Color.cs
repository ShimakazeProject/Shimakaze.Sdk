using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 游戏内使用的颜色
/// </summary>
/// <param name="Red">红色</param>
/// <param name="Green">绿色</param>
/// <param name="Blue">蓝色</param>

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = sizeof(byte) * 3)]
public record struct Color(
    [field: FieldOffset(0)] byte Red,
    [field: FieldOffset(1)] byte Green,
    [field: FieldOffset(2)] byte Blue
)
{
    /// <summary>
    /// R8 => R5
    /// </summary>
    /// <returns>R5</returns>
    public readonly byte GetR5() => unchecked((byte)((Red & 0b00111110) << 2));
    /// <summary>
    /// G8 => G6
    /// </summary>
    /// <returns>G6</returns>
    public readonly byte GetG6() => unchecked((byte)(Green << 2));
    /// <summary>
    /// B8 => B5
    /// </summary>
    /// <returns>B5</returns>
    public readonly byte GetB5() => unchecked((byte)((Blue & 0b00111110) << 2));

    /// <summary>
    /// RGB888 => RGB565
    /// </summary>
    /// <returns>RGB565</returns>
    public readonly ushort GetRGB565()
    {
        // RRRRRRRR | GGGGGGGG | BBBBBBBB
        //   >>= 1  |          |   >>= 1
        // #RRRRRRR |   <<= 2  | #BBBBBBB
        //   <<= 3  |          |   <<= 3
        // RRRRR### | GGGGGG## | BBBBB###
        int value = 0;
        // ######## ######## ######## ########
        value |= GetR5();
        // ######## ######## ######## RRRRR###
        value <<= 8 - 3;
        // ######## ######## ###RRRRR ########
        value |= GetG6();
        // ######## ######## ###RRRRR GGGGGG##
        value <<= 8 - 2;
        // ######## #####RRR RRGGGGGG ########
        value |= GetB5();
        // ######## #####RRR RRGGGGGG BBBBB###
        value >>= 3;
        // ######## ######## RRRRRGGG GGGBBBBB
        return unchecked((ushort)value);
    }

    /// <inheritdoc/>
    public override readonly string ToString() => $"#{Red:X2}{Green:X2}{Blue:X2}";
}
