using System.Diagnostics;

using Shimakaze.Sdk;

namespace Shimakaze.Sdk.Shp;

/// <summary>
/// SHP 解码器
/// </summary>
public static class ShapeReader
{
    /// <summary>
    /// 解码流
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static unsafe ShapeImage Read(Stream input)
    {
        ReadHeader(input, out ShapeFileHeader shapeFileHeader, out ShapeFrameHeader[] shapeFrameHeaders);

        byte[] buffer = [];
        ShapeImageFrame[] frames = new ShapeImageFrame[shapeFileHeader.NumImages];
        for (int i = 0; i < shapeFrameHeaders.Length; i++)
        {
            frames[i] ??= ReadFrame(input, shapeFrameHeaders[i], ref buffer);
        }

        return new ShapeImage(shapeFileHeader, frames);
    }

    private static ShapeImageFrame ReadFrame(in Stream input, in ShapeFrameHeader frameHeader, ref byte[] buffer)
    {
        using MemoryStream indexStream = new();
        if (frameHeader.CompressionType.HasFlag(ShapeFrameCompressionType.Scanline))
        {
            // TODO: 行为可能不一致
            for (int y = 0; y < frameHeader.Height; y++)
            {
                ReadRLE(input, indexStream, ref buffer);
            }
        }
        else
        {
            ReadDirect(input, indexStream, frameHeader, ref buffer);
        }

        Debug.Assert(indexStream.Length == frameHeader.BodyLength);

        indexStream.Seek(0, SeekOrigin.Begin);

        return new(frameHeader, indexStream.ToArray());
    }

    private static void ReadRLE(in Stream input, in Stream output, ref byte[] buffer)
    {
        input.Read(out ushort length);
        length -= sizeof(ushort);
        if (buffer.Length < length)
            buffer = new byte[length];

        for (int j = 0; j < length; j++)
        {
            byte b = input.ReadAsByte();
            if (b is 0)
            {
                byte count = input.ReadAsByte();
                j++;
                for (int k = 0; k < count; k++)
                    output.WriteByte(0);
            }
            else
            {
                output.WriteByte(b);
            }
        }

    }

    private static void ReadDirect(in Stream input, in Stream output, in ShapeFrameHeader frameHeader, ref byte[] buffer)
    {
        int length = frameHeader.BodyLength;
        if (buffer.Length < length)
            buffer = new byte[length];
        input.Read(buffer.AsSpan(0, length));
        output.Write(buffer.AsSpan(0, length));
    }

    private static void ReadHeader(Stream input, out ShapeFileHeader header, out ShapeFrameHeader[] frames)
    {
        input.Read(out header);
        frames = new ShapeFrameHeader[header.NumImages];
        input.Read(frames);
    }
}
