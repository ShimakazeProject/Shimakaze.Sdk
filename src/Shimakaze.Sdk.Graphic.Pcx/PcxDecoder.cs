using System.Diagnostics.CodeAnalysis;

using Shimakaze.Sdk.Graphic;
using Shimakaze.Sdk.Graphic.Pal;
using Shimakaze.Sdk.Graphic.Pcx;
using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// PCX 解码器
/// </summary>
public sealed class PcxDecoder() : IDecoder
{
    private bool _decodedHeader;
    private PcxHeader _header;
    private int _sizeOfBody;
    private int _3TimesSizeOfBody;

    /// <summary>
    /// 色板
    /// </summary>
    public Palette? Palette { get; private set; }

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// 位每像素（颜色深度/颜色位数）
    /// </summary>
    public int BitsPerPixel { get; private set; }

    /// <inheritdoc/>
    public IImage Decode(Stream input)
    {
        DecodeHeader(input);
        PcxAsserts.IsPCX(_header);

        PcxImageFrame frame = new(Width, Height);
        PcxImage image = new(frame);

        switch (BitsPerPixel)
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
                    DecodePalette(input);
                    // 输出
                    for (int i = 0; i < _sizeOfBody; i++)
                        frame.Pixels[i] = Palette[indexes[i]];
                    break;
                }
            // 24位色
            case 24:
                {
                    byte[] source = DeRLE(input, _3TimesSizeOfBody);

                    // 缓存
                    int _3TimesWidth = Width * 3;
                    int r = _header.BytesPerPlaneLine * 0;
                    int g = _header.BytesPerPlaneLine * 1;
                    int b = _header.BytesPerPlaneLine * 2;
                    int a = _header.BytesPerPlaneLine * 3;
                    unsafe
                    {
                        fixed (Rgb24* pt = frame.Pixels)
                        fixed (byte* ps = source)
                        {
                            byte* p = (byte*)pt;
                            for (int y = 0; y < Height; y++)
                            {
                                int sy = y * _3TimesWidth;
                                for (int x = 0; x < Width; x++)
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
                throw new FormatException($"Unknown BitsPerPixel: {BitsPerPixel}");
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
        Width = _header.WindowXMax - _header.WindowXMin + 1;
        Height = _header.WindowYMax - _header.WindowYMin + 1;
        BitsPerPixel = _header.ColorPlanes * _header.BitsPerPlane;
        _sizeOfBody = _header.BytesPerPlaneLine * Height;
        _3TimesSizeOfBody = _sizeOfBody * 3;
        _decodedHeader = true;
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
    [MemberNotNull(nameof(Palette))]
    private unsafe void DecodePalette(in Stream stream)
    {
        if (BitsPerPixel is 8)
        {
            PcxAsserts.IsPalette(stream.ReadByte());
            Palette = new();
            PcxAsserts.IsNotEndOfStream(stream.Read(Palette.Colors), Palette.DefaultColorCount * 3);
        }
        else
        {
            Palette = new(16);
            fixed (void* pt = Palette.Colors)
            fixed (void* ps = _header.Palette)
                Buffer.MemoryCopy(ps, pt, 3 * 16, 3 * 16);
        }
    }

}
