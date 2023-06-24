using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VXL 文件
/// </summary>
public record struct Voxel
{
    /// <summary>
    /// 文件头
    /// </summary>
    public VoxelHeader Header { get; set; }
    /// <summary>
    /// 文件色板
    /// </summary>
    public Palette Palette { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public LimbHeader[] LimbHeads { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public LimbBody[] LimbBodies { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public LimbTailer[] LimbTails { get; set; }
}
