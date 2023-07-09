namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VPL节
/// </summary>
public readonly record struct VoxelPaletteSection
{
    /// <summary>
    /// VPL节数据（颜色索引）
    /// </summary>
    public readonly byte[] Data = new byte[256];

    /// <summary>
    /// VoxelPaletteSection
    /// </summary>
    public VoxelPaletteSection()
    {
    }

    /// <summary>
    /// 获取 VPL 节数据（颜色索引）
    /// </summary>
    /// <param name="index">位置</param>
    /// <returns>颜色索引</returns>
    public readonly byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }
}