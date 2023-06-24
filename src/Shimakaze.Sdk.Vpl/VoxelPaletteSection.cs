namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VPL节
/// </summary>
public struct VoxelPaletteSection
{
    /// <summary>
    /// VPL节数据（颜色索引）
    /// </summary>
    public unsafe fixed byte Data[256];
    /// <summary>
    /// 获取节数据中的一个颜色索引
    /// </summary>
    /// <param name="index">颜色索引的索引</param>
    /// <returns>颜色索引</returns>
    public readonly byte this[int index]
    {
        get
        {
            unsafe
            {
                return Data[index];
            }
        }
        set
        {
            unsafe
            {
                fixed (byte* ptr = Data)
                    ptr[index] = value;
            }
        }
    }
}
