using System.Diagnostics;

namespace Shimakaze.Sdk.Graphic.Pcx;

[StackTraceHidden]
internal static class PcxAsserts
{
    public static void IsPCX(in PcxHeader header)
    {
        if (header.Manufacturer is not 0x0A)
            throw new FormatException("It's Not PCX Format");
        if (header is not
            {
                Version: 5,
                Encoding: 1,
                BitsPerPlane: 1 or 2 or 4 or 8 or 24,
                //PaletteInfo: 1 or 2,
            })
            throw new NotSupportedException("Not Supported");
    }
    /// <summary>
    /// 不是未定义行为
    /// </summary>
    /// <param name="size">希望这个不是 <see langword="0"/></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void IsNotUndefined(in int size)
    {
        if (size is 0)
            throw new NotImplementedException("Undefined");
    }
    public static void IsNotEndOfStream(in int size, in int length)
    {
        if (size != length)
            throw new EndOfStreamException();
    }
    public static void IsNotEndOfStream(in int @byte)
    {
        if (@byte is -1)
            throw new EndOfStreamException();
    }
    public static void IsPalette(in int @byte)
    {
        if (@byte is not 0x0c)
            throw new FormatException();
    }
}