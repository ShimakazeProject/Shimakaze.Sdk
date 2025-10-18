using Shimakaze.Sdk.Mix.Blowfish;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix 实用工具
/// </summary>
public static class Mix
{
    /// <summary>
    /// 从流中读取所有的 Entry
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info">Mix 头结构</param>
    /// <param name="flag">文件头标记 <paramref name="noFlag"/> 为 <see langword="true"/> 则为 null</param>
    /// <param name="bodyOffset">主体起始偏移点</param>
    /// <param name="noFlag">适用于 CnC1 / RA1 的旧MIX </param>
    /// <returns></returns>
    public static MixEntry[] ReadMetadata(Stream stream, out MixMetadata info, out MixTag? flag, out int bodyOffset, bool noFlag = false)
    {
        flag = null;
        bool isEncrypted = false;
        Stream decryptedStream = stream;
        if (!noFlag)
        {
            stream.Read(out MixTag tmp);
            flag = tmp;
            if (flag!.Value.HasFlag(MixTag.ENCRYPTED))
            {
                isEncrypted = true;
                Span<byte> keySource = stackalloc byte[80];
                stream.ReadExactly(keySource);

                Span<byte> key = stackalloc byte[56];
                WSKey.Decrypt(keySource, key);

#pragma warning disable IDISP001 // Dispose created
                decryptedStream = new BlowfishStream(stream, key);
#pragma warning restore IDISP001 // Dispose created
            }
        }

        decryptedStream.Read(out info);

        int size = 6 + 12 * info.Files;
        bodyOffset = isEncrypted
            ? (size + 7) / 8 * 8
            : size;
        if (!noFlag)
            bodyOffset += sizeof(MixTag);

        MixEntry[] entries = new MixEntry[size];
        decryptedStream.Read(entries);
        return entries;
    }

    /// <summary>
    /// 写入 MIX 头
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info">Mix 头结构</param>
    /// <param name="entries"></param>
    /// <param name="flag">文件头标记（为空则不写入）</param>
    /// <param name="key"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void WriteMetadata(Stream stream, in MixMetadata info, ReadOnlySpan<MixEntry> entries, MixTag? flag = MixTag.NONE, ReadOnlySpan<byte> key = default)
    {
        Stream encryptedStream = stream;
        if (flag is not null)
        {
            encryptedStream.Write(flag.Value);
            if (flag.Value.HasFlag(MixTag.ENCRYPTED))
            {
                if (key.IsEmpty)
                    throw new ArgumentException("Key cannot be empty.", nameof(key));

#pragma warning disable IDISP001 // Dispose created
                encryptedStream = new BlowfishStream(stream, key);
#pragma warning restore IDISP001 // Dispose created
            }
        }

        encryptedStream.Write(info);
        encryptedStream.Write(entries);
    }

    /// <summary>
    /// 从流中读取文件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="bodyOffset"></param>
    /// <param name="entry"></param>
    /// <param name="destination"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ReadFileAsync(Stream stream, int bodyOffset, MixEntry entry, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD
        Memory<byte> buffer = new byte[4096];
#else
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(4096); 
#endif
        int todo = entry.Size;
        float work = 0;
        int offset = bodyOffset + entry.Offset;
        stream.Seek(offset, SeekOrigin.Begin);

        while (todo > 0)
        {
            int size = Math.Min(todo, buffer.Length);
            await stream.ReadExactlyAsync(buffer[..size], cancellationToken);
            await destination.WriteAsync(buffer[..size], cancellationToken);
            work += size;
            progress?.Report(work / entry.Size);
        }
    }

    /// <summary>
    /// 向流中写入文件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="bodyOffset"></param>
    /// <param name="entry"></param>
    /// <param name="source">文件流</param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task WriteFileAsync(Stream stream, int bodyOffset, MixEntry entry, Stream source, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD
        Memory<byte> buffer = new byte[4096];
#else
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(4096); 
#endif
        int todo = entry.Size;
        float work = 0;
        int offset = bodyOffset + entry.Offset;
        stream.Seek(offset, SeekOrigin.Begin);

        while (todo > 0)
        {
            int size = Math.Min(todo, buffer.Length);
            await source.ReadExactlyAsync(buffer[..size], cancellationToken);
            await stream.WriteAsync(buffer[..size], cancellationToken);
            work += size;
            progress?.Report(work / entry.Size);
        }
    }

}
