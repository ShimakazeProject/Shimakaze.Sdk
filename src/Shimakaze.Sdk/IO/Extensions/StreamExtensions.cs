namespace Shimakaze.Sdk.IO.Extensions;

/// <summary>
/// 流实用工具
/// </summary>
public static unsafe class StreamExtensions
{
    /// <summary>
    /// 读取结构体
    /// </summary>
    /// <typeparam name="T">非托管结构体</typeparam>
    /// <param name="stream">流</param>
    /// <param name="destination">目标结构体</param>
    /// <exception cref="OverflowException"></exception>
    public static void Read<T>(this Stream stream, out T destination)
            where T : unmanaged
    {
        fixed (T* ptr = &destination)
            stream.Read(new Span<byte>(ptr, sizeof(T)));
    }

    /// <summary>
    /// 读取到数组
    /// </summary>
    /// <typeparam name="T">非托管结构体</typeparam>
    /// <param name="stream">流</param>
    /// <param name="destination">目标数组</param>
    /// <exception cref="OverflowException"></exception>
    public static void Read<T>(this Stream stream, in T[] destination)
        where T : unmanaged
    {
        fixed (T* ptr = destination)
            stream.Read(new Span<byte>(ptr, destination.Length * sizeof(T)));
    }

    /// <summary>
    /// 写入一个结构体到流
    /// </summary>
    /// <typeparam name="T">非托管结构体</typeparam>
    /// <param name="stream">流</param>
    /// <param name="value">结构体</param>
    /// <exception cref="OverflowException"></exception>
    public static void Write<T>(this Stream stream, in T value)
        where T : unmanaged
    {
        fixed (T* ptr = &value)
            stream.Write(new Span<byte>(ptr, sizeof(T)));
    }

    /// <summary>
    /// 写入一个结构体数组到流
    /// </summary>
    /// <typeparam name="T">非托管结构体</typeparam>
    /// <param name="stream">流</param>
    /// <param name="value">结构体数组</param>
    /// <exception cref="OverflowException"></exception>
    public static void Write<T>(this Stream stream, in T[] value)
        where T : unmanaged
    {
        fixed (T* ptr = value)
            stream.Write(new Span<byte>(ptr, value.Length * sizeof(T)));
    }
}