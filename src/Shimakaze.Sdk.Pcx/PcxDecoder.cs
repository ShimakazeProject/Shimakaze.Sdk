using Shimakaze.Sdk;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// PCX 解码器
/// </summary>
public sealed class PcxDecoder()
{
    private PcxHeader _header;
    private int _sizeOfBody;
    private int _3TimesSizeOfBody;

    /// <summary>
    /// 解码图片
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="FormatException"></exception>
    public PcxImage Decode(Stream input)
    {
        DecodeHeader(input);
        PcxAsserts.IsPCX(_header);

        PcxImage image = new(_header);

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
                    var indexes = DeRLE(input, _sizeOfBody);
                    // 读调色板
                    DecodePalette(input, image);
                    if (image.Palette is null)
                        throw new InvalidDataException();

                    // 输出
                    unsafe
                    {
                        for (int i = 0; i < _sizeOfBody; i++)
                        {
                            image.Pixels[i] = image.Palette[indexes[i]];
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
                    int r = _header.BytesPerPlaneLine * 0;
                    int g = _header.BytesPerPlaneLine * 1;
                    int b = _header.BytesPerPlaneLine * 2;
                    int a = _header.BytesPerPlaneLine * 3;
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

    private unsafe void DecodeHeader(in Stream stream)
    {
        int size;
        fixed (PcxHeader* p = &_header)
            size = stream.Read(new Span<byte>(p, sizeof(PcxHeader)));
        if (size != sizeof(PcxHeader))
            throw new EndOfStreamException();

        _sizeOfBody = _header.BytesPerPlaneLine * (_header.WindowYMax - _header.WindowYMin + 1);
        _3TimesSizeOfBody = _sizeOfBody * 3;
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
            int flag = stream.ReadByte();
            PcxAsserts.IsNotEndOfStream(flag);
            if ((flag & 0b11000000) is 0b11000000)
            {
                int size = flag & 0b00111111;
                PcxAsserts.IsNotUndefined(size);

                int b = stream.ReadByte();
                PcxAsserts.IsNotEndOfStream(b);

                for (int i = 0; i < size; i++)
                    data[p + i] = (byte)b;

                p += size;
            }
            else
            {
                data[p] = (byte)flag;
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
    private unsafe void DecodePalette(in Stream stream, in PcxImage image)
    {
        if (image.BitsPerPixel is 8)
        {
            PcxAsserts.IsPalette(stream.ReadByte());
            image.Palette = new();
            PcxAsserts.IsNotEndOfStream(stream.Read(image.Palette.Colors), Palette.DefaultColorCount * 3);
        }
        else
        {
            image.Palette = new(16);
            fixed (void* pt = image.Palette.Colors)
            fixed (void* ps = _header.Palette)
                Buffer.MemoryCopy(ps, pt, 3 * 16, 3 * 16);
        }
    }

}
