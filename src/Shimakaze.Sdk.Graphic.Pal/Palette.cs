using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Pal;

/// <summary>
/// 色板
/// </summary>
public record class Palette
{
    /// <summary>
    /// 颜色数量
    /// </summary>
    public const int DefaultColorCount = 256;

    /// <summary>
    /// 创建默认长度的色板
    /// </summary>
    public Palette() : this(DefaultColorCount)
    {
    }

    /// <summary>
    /// 创建具有指定长度的色板
    /// </summary>
    public Palette(int size) : this(new Rgb24[size])
    {
    }

    /// <summary>
    /// 从颜色数组中创建色板
    /// </summary>
    /// <param name="colors"></param>
    public Palette(Rgb24[] colors)
    {
        Colors = colors;
    }


    /// <summary>
    /// 颜色
    /// </summary>
    public Rgb24[] Colors { get; }

    /// <summary>
    /// 颜色
    /// </summary>
    /// <param name="index"> 索引 </param>
    /// <returns> 颜色 </returns>
    public Rgb24 this[int index]
    {
        get => Colors[index];
        set => Colors[index] = value;
    }
}