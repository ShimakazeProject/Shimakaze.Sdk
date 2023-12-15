using System.Diagnostics.CodeAnalysis;

using Shimakaze.Sdk.Graphic.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// PCX 解码器
/// </summary>
/// <param name="stream"></param>
/// <param name="leaveOpen"></param>
public sealed class PcxDecoder(Stream stream, bool leaveOpen = false) : IDisposable
{
    private bool _decodedHeader;
    private PcxHeader _header;
    private int _sizeOfBody;
    private int _3TimesSizeOfBody;

    /// <summary>
    /// Pcx头数据
    /// </summary>
    public ref PcxHeader Header
    {
        get
        {
            if (!_decodedHeader)
                DecodeHeader();
            return ref _header;
        }
    }

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

    /// <summary>
    /// 从流中解码图片
    /// </summary>
    /// <returns>Bgr24内容</returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public void Decode(Stream output)
    {
        DecodeHeader();
        PcxAsserts.IsPCX(_header);

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
                    var indexes = DeRLE(_sizeOfBody);
                    // 读调色板
                    DecodePalette();
                    // 输出
                    for (int i = 0; i < _sizeOfBody; i++)
                        output.Write(Palette[indexes[i]]);
                    break;
                }
            // 24位色
            case 24:
                {
                    byte[] source = DeRLE(_3TimesSizeOfBody);

                    // 缓存
                    int _3TimesWidth = Width * 3;
                    int r = _header.BytesPerPlaneLine * 0;
                    int g = _header.BytesPerPlaneLine * 1;
                    int b = _header.BytesPerPlaneLine * 2;
                    int a = _header.BytesPerPlaneLine * 3;
                    unsafe
                    {
                        fixed (byte* ps = source)
                        {
                            for (int y = 0; y < Height; y++)
                            {
                                int sy = y * _3TimesWidth;
                                for (int x = 0; x < Width; x++)
                                {
                                    int si = sy + x;
                                    output.WriteByte(ps[si + r]);
                                    output.WriteByte(ps[si + g]);
                                    output.WriteByte(ps[si + b]);
                                }
                            }
                        }
                    }
                    break;
                }
            default:
                throw new FormatException($"Unknown BitsPerPixel: {BitsPerPixel}");
        }
    }

    private unsafe void DecodeHeader()
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
    /// <param name="length">应该读出多少字节</param>
    private byte[] DeRLE(in int length)
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
    private unsafe void DecodePalette()
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!leaveOpen)
            stream.Dispose();
    }

}
