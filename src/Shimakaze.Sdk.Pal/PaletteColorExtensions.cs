namespace Shimakaze.Sdk.Pal;

internal static class PaletteColorExtensions
{
    /// <summary>
    /// 实际展示出来的颜色 (每个颜色的RGB值分别左移两位)
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static PaletteColor AsDisplaydColor(this in PaletteColor pixel) => new(
        unchecked((byte)((pixel.Red << 2) & 0b11111000)),
        unchecked((byte)(pixel.Green << 2)),
        unchecked((byte)((pixel.Blue << 2) & 0b11111000)));

    /// <summary>
    /// 存储到文件中的颜色 (每个颜色的RGB值分别右移两位)
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static PaletteColor AsSavedColor(this in PaletteColor pixel) => new(
        unchecked((byte)(pixel.Red >> 2)),
        unchecked((byte)(pixel.Green >> 2)),
        unchecked((byte)(pixel.Blue >> 2)));

    /// <summary>
    /// 将24位色转换为16位色
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static ushort AsRGB565(this in PaletteColor pixel)
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
}