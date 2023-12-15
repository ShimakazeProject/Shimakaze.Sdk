namespace Shimakaze.Sdk;

/// <summary>
/// 流实用工具
/// </summary>
internal static unsafe class StreamExtensions
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
        fixed (T* ptr = &destination)
            stream.Read(new Span<byte>(ptr, sizeof(T)));
    }

    /// <summary>
    /// 读取到数组
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="destination"> 目标数组 </param>
    /// <exception cref="OverflowException"> </exception>
    public static void Read<T>(this Stream stream, in T[] destination)
        where T : unmanaged
    {
        fixed (T* ptr = destination)
            stream.Read(new Span<byte>(ptr, destination.Length * sizeof(T)));
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="stream"> 流 </param>
    /// <param name="value"> 读出来的字符串 </param>
    /// <param name="length"> 要读取的长度 </param>
    /// <param name="isUnicode"> 是否是wchar </param>
    public static void Read(this Stream stream, out string value, int length, bool isUnicode = false)
    {
        if (isUnicode)
        {
            char* buffer = stackalloc char[length];
            stream.Read(new Span<byte>(buffer, length * sizeof(char)));
            value = new(buffer, 0, length);
        }
        else
        {
            sbyte* buffer = stackalloc sbyte[length];
            stream.Read(new Span<byte>(buffer, length));
            value = new(buffer, 0, length);
        }
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
        fixed (T* ptr = &value)
            stream.Write(new Span<byte>(ptr, sizeof(T)));
    }

    /// <summary>
    /// 写入一个结构体数组到流
    /// </summary>
    /// <typeparam name="T"> 非托管结构体 </typeparam>
    /// <param name="stream"> 流 </param>
    /// <param name="value"> 结构体数组 </param>
    /// <exception cref="OverflowException"> </exception>
    public static void Write<T>(this Stream stream, in T[] value)
        where T : unmanaged
    {
        fixed (T* ptr = value)
            stream.Write(new Span<byte>(ptr, value.Length * sizeof(T)));
    }

    /// <summary>
    /// 写入一个字符串到流
    /// </summary>
    /// <param name="stream"> 流 </param>
    /// <param name="value"> 字符串 </param>
    /// <param name="length"> 字符串长度 </param>
    /// <param name="isUnicode"> 是否是wchar </param>
    public static void Write(this Stream stream, string value, int length, bool isUnicode = false)
    {
        if (isUnicode)
        {
            fixed (char* ptr = value)
                stream.Write(new Span<byte>(ptr, length * sizeof(char)));
        }
        else
        {
            byte* ptr = stackalloc byte[length];
            fixed (char* p = value)
                for (int i = 0; i < length; i++)
                    ptr[i] = (byte)p[i];

            stream.Write(new Span<byte>(ptr, length));
        }
    }
}