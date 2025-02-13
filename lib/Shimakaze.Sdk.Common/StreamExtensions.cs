using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Common;

/// <summary>
/// 流实用工具
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// 断言流可以Seek
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Stream CanSeek(this Stream stream)
    {
        StreamAsserts.CanSeek(stream);
        return stream;
    }

    /// <summary>
    /// 读取结构体
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="destination"> 目标结构体 </param>
    /// <exception cref="OverflowException"> </exception>
    public static void Read<T>(this Stream stream, out T destination)
            where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        stream.ReadExactly(buffer);
        destination = MemoryMarshal.Read<T>(buffer);
    }

    /// <summary>
    /// 读取到数组
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="destination"> 目标数组 </param>
    /// <exception cref="OverflowException"> </exception>
    public static void Read<T>(this Stream stream, in Span<T> destination)
        where T : unmanaged
    {
        stream.ReadExactly(MemoryMarshal.Cast<T, byte>(destination));
    }

    /// <summary>
    /// 写入一个结构体到流
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="value"> 结构体 </param>
    /// <exception cref="OverflowException"> </exception>
    public static void Write<T>(this Stream stream, in T value)
        where T : unmanaged
    {
        stream.Write(MemoryMarshal.AsBytes([value]));
    }

    /// <summary>
    /// 写入一个结构体数组到流
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="value"> 结构体数组 </param>
    /// <exception cref="OverflowException"> </exception>
    public static void Write<T>(this Stream stream, in ReadOnlySpan<T> value)
        where T : unmanaged
    {
        stream.Write(MemoryMarshal.AsBytes(value));
    }

    /// <summary>
    /// 读取一个字节
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="EndOfStreamException">流已结束</exception>
    public static byte ReadAsByte(this Stream stream)
    {
        int b = stream.ReadByte();
        StreamAsserts.EndOfStream(b);
        return (byte)b;
    }
}
