using System.Diagnostics;

using Shimakaze.Sdk.Mix.Blowfish;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix 文件读取器
/// </summary>
public sealed class MixReader : IDisposable, IAsyncDisposable
{
    private readonly bool _isEncrypted;
    private readonly Stream _decryptedStream;
    private readonly bool _leaveOpen;
    private bool _disposedValue;

    /// <summary>
    /// 主体部分偏移位置
    /// </summary>
    public long BodyOffset { get; private set; }

    /// <summary>
    /// 主体部分文件大小
    /// </summary>
    public int BodySize { get; private set; }

    /// <summary>
    /// Entry的数量
    /// </summary>
    public short Count { get; private set; }

    /// <summary>
    /// 创建一个 Mix 文件读取器
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="leaveOpen"></param>
    /// <param name="noFlag"></param>
    public MixReader(Stream stream, bool leaveOpen = false, bool noFlag = false)
    {
        _leaveOpen = leaveOpen;
        _isEncrypted = false;
        if (!noFlag)
        {
            // 标识符
            stream.Read(out MixTag flag);
            if (flag.HasFlag(MixTag.ENCRYPTED))
            {
                _isEncrypted = true;

                Span<byte> keySource = stackalloc byte[80];
                stream.ReadExactly(keySource);

                Span<byte> key = stackalloc byte[56];
                WSKey.Decrypt(keySource, key);

                _decryptedStream = new BlowfishStream(stream, key);
            }
            else
            {
                // 未加密
                _decryptedStream = stream;
            }
        }
        else
        {
            // 未加密
            _decryptedStream = stream;
        }
        var zero = _decryptedStream.Position;

        _decryptedStream.Read(out MixMetadata info);

        Count = info.Files;
        BodySize = info.Size;

        int size = 6 + 12 * Count;
        BodyOffset = zero + (_isEncrypted
            ? (size + 7) / 8 * 8 // 整数魔法
            : size);
    }

    /// <summary>
    /// 读取所有的Entry
    /// </summary>
    /// <returns></returns>
    public MixEntry[] ReadEntries()
    {
        Span<MixEntry> entries = stackalloc MixEntry[Count];
        ReadEntries(entries);
        return entries.ToArray();
    }

    /// <summary>
    /// 读取所有Entry
    /// </summary>
    /// <param name="entries"></param>
    /// <exception cref="EndOfEntryTableException"></exception>
    public void ReadEntries(Span<MixEntry> entries)
    {
        if (_decryptedStream.Position >= BodyOffset)
            throw new EndOfEntryTableException();

        _decryptedStream.Read(entries);
    }

    /// <summary>
    /// 读取一个Entry
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="data"></param>
    public void ReadFile(MixEntry entry, out byte[] data)
    {
        data = new byte[entry.Size];
        ReadFile(entry, data.AsSpan());
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="data"></param>
    public void ReadFile(MixEntry entry, Span<byte> data)
    {
        var blowfish = _decryptedStream as BlowfishStream;
        if (blowfish is not null)
            blowfish.UnsafeSeek(BodyOffset, SeekOrigin.Begin);
        else
            _decryptedStream.Seek(BodyOffset, SeekOrigin.Begin);

        if (_isEncrypted)
            Debug.Assert(entry.Offset % 8 is 0);

        if (blowfish is not null)
            blowfish.UnsafeSeek(entry.Offset, SeekOrigin.Current);
        else
            _decryptedStream.Seek(entry.Offset, SeekOrigin.Current);

        _decryptedStream.ReadExactly(data);
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            if (!_leaveOpen)
                _decryptedStream.Dispose();
        }

        // TODO: 释放未托管的资源(未托管的对象)并重写终结器
        // TODO: 将大型字段设置为 null
        _disposedValue = true;
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await _decryptedStream.DisposeAsync().ConfigureAwait(false);
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
