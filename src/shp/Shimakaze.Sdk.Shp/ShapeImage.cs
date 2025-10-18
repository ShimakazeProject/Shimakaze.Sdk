using System.Diagnostics;

namespace Shimakaze.Sdk.Shp;

/// <summary>
/// SHP图像
/// </summary>

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class ShapeImage
{
    private readonly ShapeFileHeader _metadata;

    /// <summary>
    /// 创建SHP图像
    /// </summary>
    /// <param name="metadata">图像元数据</param>
    /// <param name="frames">图像帧</param>
    public ShapeImage(ShapeFileHeader metadata, IReadOnlyList<ShapeImageFrame> frames)
    {
        _metadata = metadata;
        ref ShapeFileHeader a = ref _metadata;
        a.NumImages = (ushort)frames.Count;
        Frames = frames;
    }

    /// <summary>
    /// SHP文件元数据
    /// </summary>
    public ShapeFileHeader Metadata => _metadata;
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


    private string GetDebuggerDisplay()
    {
        return Metadata.ToString();
    }

    /// <summary>
    /// 从流中读取SHP图像
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static unsafe ShapeImage ReadFrom(Stream input)
    {
        input.Read(out ShapeFileHeader shapeFileHeader);
        Memory<ShapeFrameHeader> shapeFrameHeaders = Array.FastCreate<ShapeFrameHeader>(shapeFileHeader.NumImages);
        input.Read(shapeFrameHeaders);

        ShapeImageFrame[] frames = Array.FastCreate<ShapeImageFrame>(shapeFileHeader.NumImages);
        for (int i = 0; i < shapeFrameHeaders.Length; i++)
            frames[i] ??= ShapeImageFrame.ReadFrom(input, shapeFrameHeaders.Span[i]);

        return new(shapeFileHeader, frames);
    }

    /// <summary>
    /// 计算帧偏移
    /// </summary>
    public void CalcOffset()
    {
        uint offset = ShapeFileHeader.Size;
        offset += (uint)Metadata.NumImages * ShapeFrameHeader.Size;
        for (int i = 0; i < Metadata.NumImages; i++)
        {
            var frame = Frames[i];
            var metadata = frame.Metadata;
            if (frame.Indexes is { Length: 0 })
            {
                metadata.Offset = 0;
            }
            else
            {
                metadata.Offset = offset;
                offset += (uint)frame.Indexes.Length;
            }
            frame.Metadata = metadata;
        }
    }
    /// <summary>
    /// 写入SHP到流
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
    {
        CalcOffset();
        stream.Write(Metadata);
        foreach (var frame in Frames.Select(i => i.Metadata))
            stream.Write(frame);
        foreach (var frame in Frames)
            frame.WriteTo(stream);
    }
}
