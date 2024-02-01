namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 调色板写入器
/// </summary>
public sealed class PaletteWriter
{
    /// <summary>
    /// 写入调色板
    /// </summary>
    public static void Write(in Palette palette, Stream stream, bool skipPreprocess = false)
    {
        if (!skipPreprocess)
            PreProcess(palette);

        stream.Write(palette.Colors);
    }

    /// <summary>
    /// 正常展示使用的颜色需要右移两位才能变成pal文件中保存的颜色。
    /// </summary>
    /// <param name="palette">调色板</param>
    public static unsafe void PreProcess(in Palette palette)
    {
        int length = palette.Count * PaletteColor.Size;
        fixed (void* ptr = palette.Colors)
        {
            byte* p = (byte*)ptr;
            for (var i = 0; i < length; i++)
            {
                p[i] >>= 2;
            }
        }
    }
}