using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// 表示一个PCX图像
/// </summary>
/// <remarks>
/// PCX是单帧图像，只有一个帧
/// </remarks>
/// <param name="metadata">元数据</param>
public abstract class PcxImage(PcxHeader metadata)
{
    /// <summary>
    /// 图像元数据
    /// </summary>
    public PcxHeader Metadata { get; } = metadata;

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int Width => Metadata.Width;

    /// <summary>
    /// 图像高度
    /// </summary>
    public int Height => Metadata.Height;

    /// <summary>
    /// 位每像素（颜色深度/颜色位数）
    /// </summary>
    public int BitsPerPixel => Metadata.BitsPerPixel;

    /// <summary>
    /// 获取像素数据
    /// </summary>
    public abstract IEnumerable<PaletteColor> GetPixels();


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
        input.Read(out PcxHeader header);
        PcxAsserts.IsPCX(header);

        switch (header.BitsPerPixel)
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
                    byte[] indexes = new byte[header.SizeOfBody];
                    using PcxRLEStream rle = new(input, true);
                    rle.ReadExactly(indexes);
                    // 读调色板
                    var palette = DecodePalette(input, header);

                    return new Pcx8BitsImage(header, indexes, palette);
                }
            // 24位色
            case 24:
                {
                    Pcx24BitsImage image = new(header);
                    byte[] source = new byte[header.SizeOfBody * 3];
                    using PcxRLEStream rle = new(input, true);
                    rle.ReadExactly(source);

                    // 缓存
                    int _3TimesWidth = header.Width * 3;
                    int r = header.BytesPerPlaneLine * 0;
                    int g = header.BytesPerPlaneLine * 1;
                    int b = header.BytesPerPlaneLine * 2;
                    int a = header.BytesPerPlaneLine * 3;
                    unsafe
                    {
                        fixed (PaletteColor* pt = image.Pixels.Span)
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
                    return image;
                }
            default:
                throw new FormatException($"Unknown BitsPerPixel: {header.BitsPerPixel}");
        }
    }

    /// <summary>
    /// 解码色板
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="EndOfStreamException"></exception>
    private static unsafe Palette DecodePalette(in Stream stream, in PcxHeader header)
    {
        if (header.BitsPerPixel is 8)
        {
            PcxAsserts.IsPalette(stream.ReadByte());
            return Palette.ReadFrom(stream);
        }
        else
        {
            fixed (void* ps = header.Palette)
            {
                var span = new Span<PaletteColor>(ps, 16);
                return new(span.ToArray());
            }
        }
    }
}
