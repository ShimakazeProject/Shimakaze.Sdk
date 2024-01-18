using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Graphic.Shp;

/// <summary>
/// SHP帧数据头
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct ShapeFrameHeader
{
    /// <summary>
    /// 0,0 的水平位置
    /// </summary>
    public ushort X;
    /// <summary>
    /// 0,0 的垂直位置
    /// </summary>
    public ushort Y;
    /// <summary>
    /// 帧的宽度。请注意，<see cref="X"/> + <see cref="Width"/> &lt; <see cref="ShapeFileHeader.Width"/>
    /// </summary>
    public ushort Width;
    /// <summary>
    /// 帧的高度。请注意，<see cref="Y"/> + <see cref="Height"/> &lt; <see cref="ShapeFileHeader.Height"/>
    /// </summary>
    public ushort Height;
    /// <summary>
    /// 特殊标志。
    /// </summary>
    /// <remarks>
    /// 它可以是任何数字
    /// </remarks>
    public ShapeFrameCompressionType CompressionType;
    /// <summary>
    /// 与 dword 对齐 3 个字节
    /// </summary>
    public byte Padding1;
    /// <inheritdoc cref="Padding1"/>
    public ushort Padding2;

    /// <summary>
    /// 
    /// </summary>
    public uint Color;

    /// <inheritdoc cref="ShapeFileHeader.Reserved"/>
    public uint Reserved;
    /// <summary>
    /// 帧数据在文件内部的位置。将此值与 Seek 一起使用以访问帧数据。如果偏移量等于 0，则为 NULL“帧”
    /// </summary>
    public uint Offset;

    /// <summary>
    /// 主体长度
    /// </summary>
    public readonly int BodyLength => Width * Height;

}
