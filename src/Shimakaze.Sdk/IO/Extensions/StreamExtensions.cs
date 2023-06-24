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
    /// <param name="buffer">缓冲区</param>
    /// <param name="destination">目标结构体</param>
    /// <param name="start">缓冲区起始位置</param>
    /// <param name="length">缓冲区可用长度</param>
    /// <exception cref="OverflowException"></exception>
    public static void Read<T>(this Stream stream, byte[] buffer, out T destination, int start = 0, int? length = null)
            where T : unmanaged
    {
        length ??= buffer.Length - start;
        // 结构体长度
        int tSize = sizeof(T);

        if (length < tSize)
            throw new OverflowException("Buffer is too short!");

        stream.Read(buffer.AsSpan(start, tSize));
        fixed (byte* ptr = buffer)
            destination = *(T*)ptr;
    }

    /// <summary>
    /// 读取结构体
    /// </summary>
    /// <typeparam name="T">非托管结构体</typeparam>
    /// <param name="stream">流</param>
    /// <param name="buffer">缓冲区</param>
    /// <param name="start">缓冲区起始位置</param>
    /// <param name="length">缓冲区可用长度</param>
    /// <returns>值</returns>
    /// <exception cref="OverflowException"></exception>
    public static T ReadAs<T>(this Stream stream, byte[] buffer, int start = 0, int? length = null)
            where T : unmanaged
    {
        length ??= buffer.Length - start;
        // 结构体长度
        int tSize = sizeof(T);

        if (length < tSize)
            throw new OverflowException("Buffer is too short!");

        stream.Read(buffer.AsSpan(start, tSize));
        fixed (byte* ptr = buffer)
            return *(T*)ptr;
    }

    /// <summary>
    /// 读取到数组
    /// </summary>
    /// <typeparam name="T">非托管结构体</typeparam>
    /// <param name="stream">流</param>
    /// <param name="buffer">缓冲区</param>
    /// <param name="destination">目标数组</param>
    /// <param name="start">缓冲区起始位置</param>
    /// <param name="length">缓冲区可用长度</param>
    /// <exception cref="OverflowException"></exception>
    public static void Read<T>(this Stream stream, byte[] buffer, T[] destination, int start = 0, int? length = null)
        where T : unmanaged
    {
        length ??= buffer.Length - start;
        // 结构体长度
        int tSize = sizeof(T);

        if (length < tSize)
            throw new OverflowException("Buffer is too short!");

        // 使缓冲区可用长度始终是 结构体长度 的倍数
        if (length % tSize is not 0)
            length = (length / tSize) * tSize;

        for (int i = 0; i < destination.Length;)
        {
            // 剩余个数
            int tRemain = destination.Length - i;
            // 剩余长度
            int tRemainSize = tRemain * tSize;
            // 读取长度 如果 剩余长度 > 缓冲区长度 则使用 缓冲区长度 否则使用 剩余长度
            int size = tRemainSize > length ? length.Value : tRemainSize;

            // 读取数据
            stream.Read(buffer.AsSpan(start, size));

            // 复制内存
            fixed (byte* pSrc = buffer)
            fixed (T* pDest = destination)
                Buffer.MemoryCopy(
                    pSrc + start,
                    pDest + i,
                    size,
                    size
                );

            i += size / tSize;
        }
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
        // 结构体长度
        int tSize = sizeof(T);
        fixed (T* ptr = &value)
            stream.Write(new Span<byte>(ptr, tSize));
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
        // 结构体长度
        int tSize = sizeof(T);
        fixed (T* ptr = value)
            stream.Write(new Span<byte>(ptr, tSize * value.Length));
    }
}