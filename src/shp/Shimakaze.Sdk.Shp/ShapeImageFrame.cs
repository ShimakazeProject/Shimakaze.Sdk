using System.Diagnostics;

namespace Shimakaze.Sdk.Shp;

/// <summary>
/// Shape 帧
/// </summary>
/// <param name="metadata">帧元数据</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class ShapeImageFrame(ShapeFrameHeader metadata)
{
    /// <summary>
    /// Shape 帧
    /// </summary>
    /// <param name="metadata">帧元数据</param>
    /// <param name="data">数据</param>
    public ShapeImageFrame(ShapeFrameHeader metadata, Memory<byte> data) : this(metadata)
    {
        Indexes = data;
    }
    /// <summary>
    /// SHP帧元数据
    /// </summary>
    public ShapeFrameHeader Metadata { get; internal set; } = metadata;

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width => Metadata.Width;

    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height => Metadata.Height;

    /// <summary>
    /// 直接获取像素数据
    /// </summary>
    public Memory<byte> Indexes { get; } = new byte[metadata.Width * metadata.Height];

    /// <summary>
    /// 是否为空帧
    /// </summary>
    public bool IsEmpty => Indexes is { Length: 0 };

    /// <summary>
    /// 写入索引数据到流
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
    {
        if (IsEmpty)
            return;

        stream.Write(Indexes);
    }

    /// <summary>
    /// 裁剪并使用RLE压缩
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ShapeImageFrame TrimAndCompress()
    {
        if (Metadata.CompressionType.HasFlag(ShapeFrameCompressionType.Scanline))
        {
            throw new InvalidOperationException();
        }

        ShapeFrameHeader metadata = Metadata;
        TrimCore(Indexes.Span, ref metadata, out var data);
        CompressCore(data, ref metadata, out data);

        return new(metadata, data.ToArray());
    }
    /// <summary>
    /// 使用RLE压缩
    /// </summary>
    /// <returns>新实例</returns>
    /// <exception cref="InvalidOperationException">已经使用RLE压缩</exception>
    public ShapeImageFrame Compress()
    {
        if (Metadata.CompressionType.HasFlag(ShapeFrameCompressionType.Scanline))
        {
            throw new InvalidOperationException();
        }

        ShapeFrameHeader metadata = Metadata;
        CompressCore(Indexes.Span, ref metadata, out var data);

        return new(metadata, data.ToArray());
    }

    private static void CompressCore(ReadOnlySpan<byte> data, ref ShapeFrameHeader metadata, out Span<byte> newData)
    {
        metadata.CompressionType |= ShapeFrameCompressionType.ScanlineRLE;
        if (data is { Length: 0 })
        {
            newData = [];
            return;
        }

        using MemoryStream ms = new();
        for (int y = 0; y < metadata.Height; y++)
        {
            int i = y * metadata.Width;

            var row = data.Slice(i, metadata.Width);

            WriteRLE(ms, row);
        }
        newData = ms.ToArray();
    }

    private static void WriteRLE(Stream stream, ReadOnlySpan<byte> row)
    {
        // Scanline
        long baseOffset = stream.Position;
        stream.Seek(sizeof(ushort), SeekOrigin.Current);

        byte counter = 0;
        for (int i = 0; i < row.Length; i++)
        {
            byte current = row[i];
            if (current is not 0)
            {
                Flush();
                stream.WriteByte(current);
                continue;
            }

            if (counter == byte.MaxValue)
                Flush();
            counter++;
        }
        Flush();

        long currentOffset = stream.Position;
        stream.Seek(baseOffset, SeekOrigin.Begin);

        stream.Write((ushort)(currentOffset - baseOffset));
        stream.Seek(currentOffset, SeekOrigin.Begin);

        void Flush()
        {
            if (counter is 0)
                return;

            stream.WriteByte(0);
            stream.WriteByte(counter);
            counter = 0;
        }
    }

    /// <summary>
    /// 裁剪空白区域
    /// </summary>
    /// <returns>新实例</returns>
    /// <exception cref="InvalidOperationException">已经使用RLE压缩</exception>
    public ShapeImageFrame Trim()
    {
        if (Metadata.CompressionType.HasFlag(ShapeFrameCompressionType.Scanline))
            throw new InvalidOperationException();

        ShapeFrameHeader metadata = Metadata;
        TrimCore(Indexes.Span, ref metadata, out var data);
        return new(metadata, data.ToArray());
    }

    private static void TrimCore(ReadOnlySpan<byte> data, ref ShapeFrameHeader metadata, out Span<byte> newData)
    {
        var oldWidth = metadata.Width;
        List<int> lengths = new(metadata.Height);

        for (ushort y = 0; y < metadata.Height; y++)
        {
            var row = data.Slice(y * metadata.Width, metadata.Width);

            if (metadata is not { X: 0, Y: 0 })
            {
                var tmp = row.TrimStart((byte)0);
                if (tmp.Length is not 0)
                {
                    metadata.X = unchecked((ushort)(metadata.Width - tmp.Length));
                    metadata.Y = y;
                    break;
                }
            }

            row = row.TrimEnd((byte)0);
            lengths.Add(row.Length);
        }

        metadata.Width = unchecked((ushort)(lengths.Max() - metadata.X));
        metadata.Height = unchecked((ushort)lengths.FindLastIndex(static i => i is not 0));

        using MemoryStream ms = new(metadata.Width * metadata.Height);
        for (int y = metadata.Y; y < metadata.Height; y++)
            ms.Write(data.Slice(y * oldWidth + metadata.X, metadata.Width));

        ms.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        newData = ms.ToArray();
    }

    private static bool IsNotZero(in byte i)
    {
        return i is not 0;
    }

    private static bool LengthIsNotZero(in (ushort Start, ushort End, ushort Length) i)
    {
        return i is not { Length: 0 };
    }

    private static (ushort Start, ushort End, ushort Length) GetDataRange<T>(Span<T> span, Checker<T> checker)
    {
        int start = -1;
        ushort end = (ushort)span.Length;
        for (int i = 0; i < end; i++)
        {
            if (checker(span[i]))
            {
                start = i;
                break;
            }
        }

        if (start is not -1)
        {
            for (ushort i = (ushort)(end - 1); i >= start; i--)
            {
                if (checker(span[i]))
                {
                    end = i;
                    break;
                }
            }
        }
        if (start is not -1)
        {
            ushort length = (ushort)(end - start);
            return ((ushort)start, end, length);
        }
        else
        {
            return (end, 0, 0);
        }
    }
    private delegate bool Checker<T>(in T data);

    private string GetDebuggerDisplay()
    {
        return Metadata.ToString();
    }

    /// <summary>
    /// 从流中读取
    /// </summary>
    /// <param name="input"></param>
    /// <param name="frameHeader"></param>
    /// <returns></returns>
    public static ShapeImageFrame ReadFrom(in Stream input, in ShapeFrameHeader frameHeader)
    {
        using MemoryStream indexStream = new();
        if (frameHeader.CompressionType.HasFlag(ShapeFrameCompressionType.Scanline))
        {
            // TODO: 行为可能不一致
            for (int y = 0; y < frameHeader.Height; y++)
            {
                ReadRLE(input, indexStream);
            }
        }
        else
        {
            ReadDirect(input, indexStream, frameHeader);
        }

        Debug.Assert(indexStream.Length == frameHeader.BodyLength);

        indexStream.Seek(0, SeekOrigin.Begin);

        return new(frameHeader, indexStream.ToArray());
    }

    private static void ReadRLE(in Stream input, in Stream output)
    {
        input.Read(out ushort length);
        length -= sizeof(ushort);

        for (int j = 0; j < length; j++)
        {
            var b = input.ReadByte();
            if (b is -1)
                throw new EndOfStreamException();
            if (b is 0)
            {
                var count = input.ReadByte();
                if (count is -1)
                    throw new EndOfStreamException();
                j++;
                for (int k = 0; k < count; k++)
                {
                    output.WriteByte(0);
                }
            }
            else
            {
                output.WriteByte(unchecked((byte)b));
            }
        }

    }

    private static void ReadDirect(in Stream input, in Stream output, in ShapeFrameHeader frameHeader)
    {
        int length = frameHeader.BodyLength;

#if NETSTANDARD2_0
        Span<byte> buffer = new byte[4096]; 
#else
        Span<byte> buffer = GC.AllocateUninitializedArray<byte>(4096);
#endif
        while (length > 0)
        {
            var size = Math.Min(length, buffer.Length);
            input.ReadExactly(buffer[..size]);
            output.Write(buffer[..size]);
            length -= size;
        }
    }
}
