using System.Collections;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 色板
/// </summary>
/// <param name="Colors">色表</param>
public record class Palette(Memory<PaletteColor> Colors) : IEnumerable<PaletteColor>
{
    /// <summary>
    /// 颜色数量
    /// </summary>
    public const int DefaultColorCount = 256;

    /// <summary>
    /// 创建指定大小的空色板
    /// </summary>
    /// <param name="size">容量</param>
    public Palette(int size = DefaultColorCount) : this(new PaletteColor[size].AsMemory())
    {
    }

    /// <summary>
    /// 取出指定颜色
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>

    public PaletteColor this[int index]
    {
        get => Colors.Span[index];
        set => Colors.Span[index] = value;
    }

    /// <summary>
    /// 从指定流中加载
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static Palette ReadFrom(Stream stream, int length = DefaultColorCount)
    {
        Palette palette = new(length);
        stream.Read(palette.Colors);
        return palette;
    }

    /// <summary>
    /// 写入数据到流中
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
        => stream.Write(Colors);

    /// <summary>
    /// 获取所有颜色
    /// </summary>
    /// <returns></returns>
    public IEnumerator<PaletteColor> GetEnumerator()
    {
        for (int i = 0; i < Colors.Length; i++)
            yield return Colors.Span[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
