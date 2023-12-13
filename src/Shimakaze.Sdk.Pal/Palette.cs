namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 色板
/// </summary>
public readonly record struct Palette
{
    /// <summary>
    /// 颜色数量
    /// </summary>
    public const int ColorCount = 256;
    /// <summary>
    /// 颜色
    /// </summary>
    public Color[] Colors { get; } = new Color[ColorCount];

    /// <summary>
    /// 颜色
    /// </summary>
    /// <param name="index"> 索引 </param>
    /// <returns> 颜色 </returns>
    public Color this[int index]
    {
        get => Colors[index];
        set => Colors[index] = value;
    }

    /// <summary>
    /// 色板
    /// </summary>
    public Palette()
    {
    }
}