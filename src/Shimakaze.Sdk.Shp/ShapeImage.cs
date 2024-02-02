using System.Diagnostics;

namespace Shimakaze.Sdk.Shp;

/// <summary>
/// SHP图像
/// </summary>

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class ShapeImage
{
    /// <summary>
    /// 创建SHP图像
    /// </summary>
    /// <param name="metadata">图像元数据</param>
    /// <param name="frames">图像帧</param>
    public ShapeImage(ShapeFileHeader metadata, IReadOnlyList<ShapeImageFrame> frames)
    {
        metadata.NumImages = (ushort)frames.Count;
        Metadata = metadata;
        Frames = frames;
        CalcOffset();
    }

    /// <summary>
    /// SHP文件元数据
    /// </summary>
    public ShapeFileHeader Metadata { get; }

    /// <summary>
    /// 获取帧
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ShapeImageFrame this[int index] => Frames[index];

    /// <summary>
    /// 所有帧
    /// </summary>
    public IReadOnlyList<ShapeImageFrame> Frames { get; }

    /// <summary>
    /// 第一个帧
    /// </summary>
    public ShapeImageFrame RootFrame => Frames[0];

    private unsafe void CalcOffset()
    {
        uint offset = ShapeFileHeader.Size;
        offset += (uint)Metadata.NumImages * ShapeFrameHeader.Size;
        for (int i = 0; i < Metadata.NumImages; i++)
        {
            ShapeImageFrame frame = Frames[i];
            ref ShapeFrameHeader metadata = ref frame.MetadataRef;
            fixed (ShapeFrameHeader* p = &metadata)
            {
                if (frame.Indexes is { Length: 0 })
                {
                    p->Offset = 0;
                }
                else
                {
                    p->Offset = offset;
                    offset += (uint)frame.Indexes.Length;
                }
            }
        }
    }

    private string GetDebuggerDisplay()
    {
        return Metadata.ToString();
    }
}