using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// PCX 解码器
/// </summary>
public static class PcxDecoder
{
    /// <summary>
    /// 解码图片
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="FormatException"></exception>
    public static PcxImage Decode(Stream input)
    {
        DecodeHeader(input, out var header, out var sizeOfBody, out var _3TimesSizeOfBody);
        PcxAsserts.IsPCX(header);

        PcxImage image = new(header);

        switch (image.BitsPerPixel)
        {
            // 2色
            case 1:
                Console.WriteLine("2色");
                throw new NotImplementedException();
            // 4色
            case 2:
                Console.WriteLine("4色");
                throw new NotImplementedException();
            // 16色
            case 4:
                Console.WriteLine("16色");
                throw new NotImplementedException();
            // 256色
            case 8:
                {
                    // 读取主体
                    byte[] indexes = DeRLE(input, sizeOfBody);
                    // 读调色板
                    DecodePalette(input, image, header);
                    if (image.Palette is null)
                    {
                        throw new InvalidDataException();
                    }

                    // 输出
                    var span = indexes.Select(i => image.Palette[i]).ToArray().AsSpan();
                    if (image.Pixels.Length == indexes.Length)
                    {
                        span.CopyTo(image.Pixels);
                    }
                    else
                    {
                        var row = indexes.Length / image.Height;
                        for (int y = 0; y < image.Height; y++)
                        {
                            span.Slice(y * row, image.Width)
                                .CopyTo(image.Pixels.AsSpan().Slice(y * image.Width, image.Width));
                        }
                    }

                    break;
                }
            // 24位色
            case 24:
                {
                    byte[] source = DeRLE(input, _3TimesSizeOfBody);

                    // 缓存
                    int _3TimesWidth = image.Width * 3;
                    int r = header.BytesPerPlaneLine * 0;
                    int g = header.BytesPerPlaneLine * 1;
                    int b = header.BytesPerPlaneLine * 2;
                    int a = header.BytesPerPlaneLine * 3;
                    unsafe
                    {
                        fixed (PaletteColor* pt = image.Pixels)
                        fixed (byte* ps = source)
                        {
                            byte* p = (byte*)pt;
                            for (int y = 0; y < image.Height; y++)
                            {
                                int sy = y * _3TimesWidth;
                                for (int x = 0; x < image.Width; x++)
                                {
                                    int si = sy + x;
                                    *p = ps[si + r];
                                    p++;
                                    *p = ps[si + g];
                                    p++;
                                    *p = ps[si + b];
                                    p++;
                                }
                            }
                        }
                    }
                    break;
                }
            default:
                throw new FormatException($"Unknown BitsPerPixel: {image.BitsPerPixel}");
        }

        return image;
    }

    private unsafe static void DecodeHeader(in Stream stream, out PcxHeader header, out int sizeOfBody, out int threeTimesSizeOfBody)
    {
        int size;
        fixed (PcxHeader* p = &header)
        {
            size = stream.Read(new Span<byte>(p, sizeof(PcxHeader)));
        }

        if (size != sizeof(PcxHeader))
        {
            throw new EndOfStreamException();
        }

        sizeOfBody = header.BytesPerPlaneLine * (header.WindowYMax - header.WindowYMin + 1);
        threeTimesSizeOfBody = sizeOfBody * 3;
    }

    /// <summary>
    /// 解码RLE
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="length">应该读出多少字节</param>
    private static byte[] DeRLE(in Stream stream, in int length)
    {
        byte[] data = new byte[length];
        for (int p = 0; p < length;)
        {
            byte flag = stream.ReadAsByte();
            if ((flag & 0b11000000) is 0b11000000)
            {
                int size = flag & 0b00111111;
                PcxAsserts.IsNotUndefined(size);

                byte b = stream.ReadAsByte();

                for (int i = 0; i < size; i++)
                {
                    data[p + i] = b;
                }

                p += size;
            }
            else
            {
                data[p] = flag;
                p++;
            }
        }
        return data;
    }

    /// <summary>
    /// 解码色板
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="EndOfStreamException"></exception>
    private unsafe static void DecodePalette(in Stream stream, in PcxImage image, in PcxHeader header)
    {
        if (image.BitsPerPixel is 8)
        {
            PcxAsserts.IsPalette(stream.ReadByte());
            image.Palette = new();
            stream.Read<PaletteColor>(image.Palette.Colors);
        }
        else
        {
            image.Palette = new(16);
            fixed (void* pt = image.Palette.Colors)
            fixed (void* ps = header.Palette)
            {
                Buffer.MemoryCopy(ps, pt, 3 * 16, 3 * 16);
            }
        }
    }

}
