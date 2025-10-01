using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// PCX 文件头
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PcxHeader
{
    /// <summary>
    /// Always 0x0A
    /// </summary>
    public byte Manufacturer;

    /// <summary>
    /// PC Paintbrush version. Acts as file format version. <br/>
    /// 0 = v2.5 <br/>
    /// 2 = v2.8 with palette <br/>
    /// 3 = v2.8 without palette <br/>
    /// 4 = Paintbrush for Windows <br/>
    /// 5 = v3.0 or higher
    /// </summary>
    public byte Version;

    /// <summary>
    /// Should be 0x01 <br/>
    /// 0 = uncompressed image (not officially allowed, but some software supports it) <br/>
    /// 1 = PCX run length encoding
    /// </summary>
    public byte Encoding;

    /// <summary>
    /// Number of bits per pixel in each entry of the colour planes (1, 2, 4, 8, 24)
    /// </summary>
    public byte BitsPerPlane;

    /// <summary>
    /// Window (image dimensions): <br/>
    /// ImageWidth = Xmax - Xmin + 1 <br/>
    /// ImageHeight = Ymax - Ymin + 1
    /// </summary>
    /// <remarks>
    /// Normally Xmin and Ymin should be set to zero. <br/>
    /// Note that these field values are valid rows and columns, <br/>
    /// which is why you have to add one to get the actual dimension <br/>
    /// (so a 200 pixel high image would have Ymin=0 and Ymax=199, <br/>
    /// or Ymin=100 and Ymax=299, etc.)
    /// </remarks>
    public ushort WindowXMin;

    /// <inheritdoc cref="WindowXMin"/>
    public ushort WindowYMin;

    /// <inheritdoc cref="WindowXMin"/>
    public ushort WindowXMax;

    /// <inheritdoc cref="WindowXMin"/>
    public ushort WindowYMax;

    /// <summary>
    /// This is supposed to specify the image's vertical and <br/>
    /// horizontal resolution in DPI (dots per inch), <br/>
    /// but it is rarely reliable. <br/>
    /// It often contains the image dimensions, or nothing at all.
    /// </summary>
    public ushort VertDPI;

    /// <inheritdoc cref="VertDPI"/>
    public ushort HorzDPI;

    /// <summary>
    /// Palette for 16 colors or less, in three-byte RGB entries.<br/>
    /// Padded with 0x00 to 48 bytes in total length.<br/>
    /// See below for more details on palette handling.
    /// </summary>
    public unsafe fixed byte Palette[48];

    /// <summary>
    /// Should be set to 0, but can sometimes contain junk.
    /// </summary>
    public byte Reserved;

    /// <summary>
    /// Number of colour planes. Multiply by BitsPerPlane to get the actual colour depth.
    /// </summary>
    public byte ColorPlanes;

    /// <summary>
    /// Number of bytes to read for a single plane's scanline, <br/>
    /// i.e. at least ImageWidth ÷ 8 bits per byte × BitsPerPlane. <br/>
    /// Must be an even number. Do not calculate from Xmax-Xmin. <br/>
    /// Normally a multiple of the machine's native word length (2 or 4)
    /// </summary>
    public ushort BytesPerPlaneLine;

    /// <summary>
    /// How to interpret palette: <br/>
    /// 1 = Color/BW <br/>
    /// 2 = Grayscale (ignored in PC Paintbrush IV and later)
    /// </summary>
    public ushort PaletteInfo;

    /// <summary>
    /// Only supported by PC Paintbrush IV or higher; <br/>
    /// deals with scrolling. Best to just ignore it.
    /// </summary>
    public ushort HorScrSize;

    /// <inheritdoc cref="HorScrSize"/>
    public ushort VerScrSize;

    /// <summary>
    /// Filler to bring header up to 128 bytes total. Can contain junk.
    /// </summary>
    public unsafe fixed byte Padding[54];

    /// <summary>
    /// Number of bits per plane.
    /// </summary>
    public readonly int BitsPerPixel => ColorPlanes * BitsPerPlane;

    /// <summary>
    /// Width of the image.
    /// </summary>
    public readonly int Width => WindowXMax - WindowXMin + 1;

    /// <summary>
    /// Height of the image.
    /// </summary>
    public readonly int Height => WindowYMax - WindowYMin + 1;

    /// <summary>
    /// Size of the image body.
    /// </summary>
    public int SizeOfBody => BytesPerPlaneLine * Height;
}
