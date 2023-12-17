namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VPL节
/// </summary>
public struct VoxelPaletteSection
{
    /// <summary>
    /// VPL节数据（颜色索引）
    /// </summary>
    internal unsafe fixed byte Data[256];

    /// <summary>
    /// 获取 VPL 节数据（颜色索引）
    /// </summary>
    /// <param name="index"> 位置 </param>
    /// <returns> 颜色索引 </returns>
    public unsafe byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }
}