using System.Collections;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pal;

/// <summary>
/// 色板
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1, Size = sizeof(byte) * 3)]
public partial struct Palette : IEnumerable<Color>
{
    /// <summary>
    /// 色板中的数据
    /// </summary>
    [FieldOffset(0)]
    public unsafe fixed byte Data[3 * COLOR_COUNT];
    /// <summary>
    /// 颜色总数
    /// </summary>
    public const int COLOR_COUNT = 256;
    /// <summary>
    /// 获取一个颜色
    /// </summary>
    /// <param name="index">索引值</param>
    /// <returns>颜色</returns>
    public readonly Color this[int index]
    {
        get
        {
            unsafe
            {
                fixed (void* ptr = Data)
                    return ((Color*)ptr)[index];
            }
        }
        set
        {
            unsafe
            {
                fixed (void* ptr = Data)
                    ((Color*)ptr)[index] = value;
            }
        }
    }

    /// <inheritdoc/>
    public readonly IEnumerator<Color> GetEnumerator()
    {
        unsafe
        {
            return new Enumerator(this);
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
