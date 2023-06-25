using System.Collections;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 色板
/// </summary>
public readonly record struct Palette
{
    /// <summary>
    /// 颜色数量
    /// </summary>
    public const int COLOR_COUNT = 256;
    /// <summary>
    /// 颜色
    /// </summary>
    public readonly Color[] Colors = new Color[COLOR_COUNT];

    /// <summary>
    /// 色板
    /// </summary>
    public Palette()
    {
    }
}