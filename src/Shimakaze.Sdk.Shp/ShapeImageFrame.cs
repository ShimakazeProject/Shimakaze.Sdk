using System;
using System.Diagnostics;

namespace Shimakaze.Sdk.Shp;

/// <summary>
/// Shape 帧
/// </summary>
/// <param name="metadata">帧元数据</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class ShapeImageFrame(ShapeFrameHeader metadata)
{
    private ShapeFrameHeader _metadata = metadata;
    /// <summary>
    /// Shape 帧
    /// </summary>
    /// <param name="metadata">帧元数据</param>
    /// <param name="data">数据</param>
    public ShapeImageFrame(ShapeFrameHeader metadata, byte[] data) : this(metadata)
    {
        Indexes = data;
    }
    /// <summary>
    /// SHP帧元数据
    /// </summary>
    public ShapeFrameHeader Metadata => _metadata;
    internal ref ShapeFrameHeader MetadataRef => ref _metadata;

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width => _metadata.Width;

    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height => _metadata.Height;

    /// <summary>
    /// 直接获取像素数据
    /// </summary>
    public byte[] Indexes { get; } = new byte[metadata.Width * metadata.Height];

    /// <summary>
    /// 写入索引数据到流
    /// </summary>
    /// <param name="stream"></param>
    public void WriteTo(Stream stream)
    {
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
            throw new InvalidOperationException();

        (ushort x, ushort y, ushort width, ushort height, byte[] data) = TrimCore(Indexes, Metadata.Width, Metadata.Height);
        data = CompressCore(data, width, height);

        var metadata = Metadata;
        metadata.X = x;
        metadata.Y = y;
        metadata.Width = width;
        metadata.Height = height;
        metadata.CompressionType |= ShapeFrameCompressionType.ScanlineRLE;

        return new(metadata, data);
    }
    /// <summary>
    /// 使用RLE压缩
    /// </summary>
    /// <returns>新实例</returns>
    /// <exception cref="InvalidOperationException">已经使用RLE压缩</exception>
    public ShapeImageFrame Compress()
    {
        if (Metadata.CompressionType.HasFlag(ShapeFrameCompressionType.Scanline))
            throw new InvalidOperationException();


        var metadata = Metadata;
        metadata.CompressionType |= ShapeFrameCompressionType.ScanlineRLE;

        return new(metadata, CompressCore(Indexes, Metadata.Width, Metadata.Height));
    }

    private static byte[] CompressCore(in byte[] data, in ushort rawWidth, in ushort rawHeight)
    {
        if (data is { Length: 0 })
            return data;

        using MemoryStream ms = new();
        for (int y = 0; y < rawHeight; y++)
        {
            int i = y * rawWidth;

            Span<byte> row = data.AsSpan(i, rawWidth);
            WriteRLE(ms, row);
        }
        return ms.ToArray();
    }

    private static void WriteRLE(in Stream stream, in Span<byte> row)
    {
        long baseOffset = stream.Position;
        stream.Seek(sizeof(ushort), SeekOrigin.Current);

        byte counter = 0;
        for (int i = 0; i < row.Length; i++)
        {
            ref byte current = ref row[i];
            if (current is 0)
            {
                counter++;
            }
            else
            {
                if (counter is not 0)
                {
                    stream.WriteByte(0);
                    Debug.Assert(counter is not 0);
                    stream.WriteByte(counter);
                    counter = 0;
                }
                stream.WriteByte(current);
            }
        }
        if (counter is not 0)
        {
            stream.WriteByte(0);
            stream.WriteByte(counter);
        }
        long currentOffset = stream.Position;
        stream.Seek(baseOffset, SeekOrigin.Begin);
        stream.Write((ushort)(currentOffset - baseOffset));
        stream.Seek(currentOffset, SeekOrigin.Begin);
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

        (ushort x, ushort y, ushort width, ushort height, byte[] data) = TrimCore(Indexes, Metadata.Width, Metadata.Height);

        var metadata = Metadata;
        metadata.X = x;
        metadata.Y = y;
        metadata.Width = width;
        metadata.Height = height;

        return new(metadata, data);
    }

    private static (ushort X, ushort Y, ushort Width, ushort Height, byte[] Data) TrimCore(in byte[] data, in ushort rawWidth, in ushort rawHeight)
    {
        (ushort Start, ushort End, ushort Length)[] maps = new (ushort Start, ushort End, ushort Length)[rawHeight];
        for (int y = 0; y < rawHeight; y++)
        {
            int i = y * rawWidth;

            Span<byte> row = data.AsSpan(i, rawWidth);
            maps[y] = GetDataRange(row, IsNotZero);
        }
        (ushort top, ushort bottom, ushort height) = GetDataRange<(ushort Start, ushort End, ushort Length)>(maps, LengthIsNotZero);
        ushort left = maps.Select(i => i.Start).Min();
        ushort right = maps.Select(i => i.End).Max();
        ushort width = (ushort)(right - left);

        using MemoryStream ms = new(width * height);
        for (int y = top; y < bottom; y++)
        {
            int i = y * rawWidth;

            Span<byte> row = data.AsSpan(i, rawWidth).Slice(left, width);
            ms.Write(row);
        }
        return (left, top, width, height, ms.ToArray());
    }

    private static bool IsNotZero(in byte i) => i is not 0;
    private static bool LengthIsNotZero(in (ushort Start, ushort End, ushort Length) i) => i is not { Length: 0 };

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
}