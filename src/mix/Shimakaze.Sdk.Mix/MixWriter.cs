using System.Diagnostics;
using System.Security.Cryptography;

using Shimakaze.Sdk.Mix.Blowfish;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix 文件写入器
/// </summary>
public sealed class MixWriter : IDisposable, IAsyncDisposable
{
    private readonly bool _isEncrypted;
    private readonly bool _noFlag;
#pragma warning disable IDISP008 // Don't assign member with injected and created disposables
    private readonly Stream _encryptedStream;
#pragma warning restore IDISP008 // Don't assign member with injected and created disposables
    private readonly bool _leaveOpen;
    private readonly long _zero;
    private bool _disposedValue;

    /// <summary>
    /// 创建一个 Mix 文件写入器
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="leaveOpen"></param>
    /// <param name="encrypt"></param>
    /// <param name="noFlag"></param>
    public MixWriter(Stream stream, bool leaveOpen = false, bool encrypt = false, bool noFlag = false)
    {
        _zero = stream.Position;
        _noFlag = noFlag;
        _leaveOpen = leaveOpen;
        if (noFlag && encrypt)
            throw new NotSupportedException("不支持不写入标记的同时加密 mix");

        if (!noFlag)
        {
            // 标识符
            MixTag tag = MixTag.NONE;
            if (encrypt)
            {
                tag |= MixTag.ENCRYPTED;
                _isEncrypted = true;
                stream.Write(tag);

                // 生成 56 字节密钥
                var key = RandomNumberGenerator.GetBytes(56);
                Span<byte> keySource = stackalloc byte[80];
                WSKey.Encrypt(key, keySource);

                stream.Write(keySource);

                _encryptedStream = new BlowfishStream(stream, key);
            }
            else
            {
                stream.Write(tag);

                // 未加密
                _encryptedStream = stream;
            }
        }
        else
        {
            // 未加密
            _encryptedStream = stream;
        }
        var zero = _encryptedStream.Position;
    }

    /// <summary>
    /// 写入所有文件
    /// </summary>
    /// <param name="files"></param>
    /// <param name="idCalculator"></param>
    public void WriteFiles(IEnumerable<FileInfo> files, IdCalculator idCalculator)
    {
        List<MixEntry> entries = new(files.Count());
        int offset = 0;
        foreach (var file in files)
        {
            MixEntry entry = new()
            {
                Id = idCalculator(file.Name),
                Offset = offset,
                Size = (int)file.Length
            };

            offset += _isEncrypted
                ? (entry.Size + 7) / 8 * 8 // 整数魔法
                : entry.Size;

            entries.Add(entry);
        }

        WriteFilesInternal(entries, files);
    }

    internal void WriteFilesInternal(IEnumerable<MixEntry> entries, IEnumerable<FileInfo> files)
    {
        WriteEntries(entries.ToArray());
        foreach (var file in files)
        {
            using var fs = file.OpenRead();
            fs.CopyTo(_encryptedStream);
        }
    }

    private void WriteEntries(ReadOnlySpan<MixEntry> entries)
    {
        Debug.Assert(entries.Length > short.MaxValue);
        int offset = 0;
        int size = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            offset = int.Max(offset, entries[i].Offset);
            size = int.Max(size, entries[i].Size);
        }

        MixMetadata metadata = new()
        {
            Files = (short)entries.Length,
            Size = offset + size
        };

        using MemoryStream ms = new();
        ms.Write(metadata);
        ms.Write(entries);
        ms.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(_encryptedStream);
    }

    private MixMetadata _metadata;

    /// <inheritdoc />
    public void Write(in MixEntry value)
    {
        if (_isEncrypted)
            throw new NotSupportedException("Cannot write to an encrypted stream. Please use WriteFiles.");

        _encryptedStream.Write(value);

        _metadata.Size = Math.Max(_metadata.Size, value.Offset + value.Size);
        _metadata.Files++;
    }


    /// <summary>
    /// 写入元数据
    /// </summary>
    public void WriteMetadata()
    {
        long current = _encryptedStream.Position;
        _encryptedStream.Seek(_zero, SeekOrigin.Begin);
        try
        {
            WriteMetadataInternal((int)MixTag.NONE, _metadata);
        }
        finally
        {
            _encryptedStream.Seek(current, SeekOrigin.Begin);
        }
    }

    /// <summary>
    /// 直接写入元数据
    /// </summary>
    /// <param name="flag"> 标记 </param>
    /// <param name="metadata"> 元数据 </param>
    internal void WriteMetadataInternal(int flag, MixMetadata metadata)
    {
        if (_isEncrypted)
            throw new NotSupportedException("Cannot write to an encrypted stream. Please use WriteFiles.");

        if (!_noFlag)
            _encryptedStream.Write(BitConverter.GetBytes(flag));
        _encryptedStream.Write(metadata);
    }


    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            if (!_leaveOpen)
                _encryptedStream.Dispose();
        }

        // TODO: 释放未托管的资源(未托管的对象)并重写终结器
        // TODO: 将大型字段设置为 null
        _disposedValue = true;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await _encryptedStream.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
    }
}
