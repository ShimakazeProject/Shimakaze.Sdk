namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 调色板读取器
/// </summary>
public static class PaletteReader
{
    /// <summary>
    /// 读取调色板
    /// </summary>
    /// <param name="stream">基础流</param>
    /// <param name="length">读取出的颜色数量</param>
    /// <param name="skipPostprocess">
    /// 跳过后处理 <br/>
    /// pal文件中保存的颜色需要左移两位才能变成正常展示使用的颜色。<br/>
    /// 设置为<see langword="true"/>则跳过左移处理。
    /// </param>
    public static Palette Read(Stream stream, int length = Palette.DefaultColorCount, bool skipPostprocess = false)
    {
        Palette palette = new(length);
        stream.Read(palette.Colors);
        if (!skipPostprocess)
            PostProcess(palette);

        return palette;
    }

    /// <summary>
    /// pal文件中保存的颜色需要左移两位才能变成正常展示使用的颜色。
    /// </summary>
    /// <param name="palette">调色板</param>
    public static unsafe void PostProcess(in Palette palette)
    {
        int length = palette.Count * PaletteColor.Size;
        fixed (void* ptr = palette.Colors)
        {
            byte* p = (byte*)ptr;
            for (var i = 0; i < length; i++)
            {
                p[i] <<= 2;
            }
        }
    }
}