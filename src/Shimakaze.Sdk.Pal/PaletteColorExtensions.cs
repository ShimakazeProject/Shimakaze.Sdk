namespace Shimakaze.Sdk.Pal;

internal static class PaletteColorExtensions
{
    /// <summary>
    /// 实际展示出来的颜色 (每个颜色的RGB值分别左移两位)
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static PaletteColor AsDisplaydColor(this PaletteColor pixel) => new(
        unchecked((byte)(pixel.Red << 2)),
        unchecked((byte)(pixel.Green << 2)),
        unchecked((byte)(pixel.Blue << 2)));

    /// <summary>
    /// 存储到文件中的颜色 (每个颜色的RGB值分别右移两位)
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    public static PaletteColor AsSavedColor(this PaletteColor pixel) => new(
        unchecked((byte)(pixel.Red >> 2)),
        unchecked((byte)(pixel.Green >> 2)),
        unchecked((byte)(pixel.Blue >> 2)));
}