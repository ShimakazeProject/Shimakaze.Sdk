using System.Runtime.InteropServices;

namespace Shimakaze.Sdk;

/// <summary>
/// 流实用工具
/// </summary>
internal static class StreamExtensions
{
    /// <summary>
    /// 读取结构体
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="destination"> 目标结构体 </param>
    public static unsafe void Read<T>(this Stream stream, out T destination)
        where T : unmanaged
    {
        fixed (T* ptr = &destination)
            stream.Read(new Span<T>(ptr, 1));
    }

    /// <inheritdoc cref="Read{T}(Stream, out T)"/>
    public static unsafe void Read<T>(this Stream stream, Span<T> destination)
        where T : unmanaged
        => stream.ReadExactly(MemoryMarshal.AsBytes(destination));

    /// <inheritdoc cref="Read{T}(Stream, out T)"/>
    public static unsafe void Read<T>(this Stream stream, Memory<T> destination)
        where T : unmanaged
        => stream.Read(destination.Span);

    /// <summary>
    /// 写入结构体到流
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="source"> 结构体 </param>
    public static void Write<T>(this Stream stream, in T source)
        where T : unmanaged
        => stream.Write(MemoryMarshal.AsBytes([source]));

    /// <inheritdoc cref="Write{T}(Stream, in T)"/>
    public static void Write<T>(this Stream stream, ReadOnlySpan<T> source)
        where T : unmanaged
        => stream.Write(MemoryMarshal.AsBytes(source));

    /// <inheritdoc cref="Write{T}(Stream, ReadOnlySpan{T})"/>
    public static void Write<T>(this Stream stream, Span<T> source)
        where T : unmanaged
        => stream.Write((ReadOnlySpan<T>)source);

    /// <inheritdoc cref="Write{T}(Stream, ReadOnlySpan{T})"/>
    public static void Write<T>(this Stream stream, ReadOnlyMemory<T> source)
        where T : unmanaged
        => stream.Write(source.Span);

    /// <inheritdoc cref="Write{T}(Stream, ReadOnlySpan{T})"/>
    public static void Write<T>(this Stream stream, Memory<T> source)
        where T : unmanaged
        => stream.Write(source.Span);
}
