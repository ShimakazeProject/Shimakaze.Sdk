using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Graphic.Shp;

/// <summary>
/// SHP文件头
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct ShapeFileHeader
{
    /// <summary>
    /// 保留 总是 <see langword="0"/>
    /// </summary>
    public ushort Reserved;
    /// <summary>
    /// 图像宽度
    /// </summary>
    public ushort Width;
    /// <summary>
    /// 图像高度
    /// </summary>
    public ushort Height;
    /// <summary>
    /// 帧数量
    /// </summary>
    public ushort NumImages;
}
