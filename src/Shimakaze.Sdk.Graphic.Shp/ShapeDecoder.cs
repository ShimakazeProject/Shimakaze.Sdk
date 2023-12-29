using System.Diagnostics.CodeAnalysis;

using Shimakaze.Sdk.Graphic.Pal;
using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Shp;

/// <summary>
/// SHP 解码器
/// </summary>
/// <param name="palette">调色板</param>
public sealed class ShapeDecoder(Palette palette) : Decoder<ShapeImage>
{
    private ShapeFileHeader _shapeFileHeader;
    private ShapeFrameHeader[]? _shapeFrameHeaders;

    /// <inheritdoc/>
    public override unsafe ShapeImage Decode(Stream input)
    {
        DecodeHeader(input);

        byte[] buffer = [];
        ShapeImageFrame[] frames = new ShapeImageFrame[_shapeFileHeader.NumImages];
        for (int i = 0; i < _shapeFrameHeaders.Length; i++)
        {
            using MemoryStream indexStream = new();
            ref ShapeFrameHeader frameHeader = ref _shapeFrameHeaders[i];
            frames[i] ??= new(frameHeader, frameHeader.Width, frameHeader.Height);

            if (frameHeader.CompressionType.HasFlag(ShapeFrameCompressionType.ScanlineRLE))
            {
                for (int y = 0; y < frameHeader.Height; y++)
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
                                indexStream.WriteByte(0);
                        }
                        else
                        {
                            indexStream.WriteByte(b);
                        }
                    }
                }
            }
            else
            {
                int length = frameHeader.BodyLength;
                if (buffer.Length < length)
                    buffer = new byte[length];
                input.Read(buffer.AsSpan(0, length));
                indexStream.Write(buffer.AsSpan(0, length));
            }

            if (indexStream.Length != frameHeader.BodyLength)
                Console.WriteLine(indexStream.Length);
            indexStream.Seek(0, SeekOrigin.Begin);


            fixed (Rgb24* ptr = frames[i].Pixels)
            {
                for (Rgb24* p = ptr; indexStream.Position < indexStream.Length; p++)
                {
                    *p = palette[indexStream.ReadByte()];
                }
            }
        }

        return new ShapeImage(_shapeFileHeader, frames);
    }

    [MemberNotNull(nameof(_shapeFrameHeaders))]
    private void DecodeHeader(Stream input)
    {
        input.Read(out _shapeFileHeader);
        _shapeFrameHeaders = new ShapeFrameHeader[_shapeFileHeader.NumImages];
        input.Read(_shapeFrameHeaders);
    }
}
