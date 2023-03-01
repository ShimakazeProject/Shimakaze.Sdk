using Shimakaze.Sdk.Data.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix文件头写入
/// </summary>
public class MixEntryWriter : IDisposable
{
    /// <summary>
    /// Mix文件流
    /// </summary>
    protected Stream _baseStream;
    private readonly byte[] _buffer = new byte[12];
    private readonly bool _leaveOpen;

    /// <summary>
    /// 流起始位置
    /// </summary>
    protected readonly long _start;
    /// <summary>
    /// 线程安全锁
    /// </summary>
    protected Mutex _mutex = new();
    private bool _disposedValue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MixEntryWriter(Stream stream, bool leaveOpen)
    {
        if (!stream.CanSeek)
            throw new NotSupportedException("The stream cannot support Seek!");
        _baseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        _start = _baseStream.Position;
        _ = OnInitAsync();
        _leaveOpen = leaveOpen;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    protected virtual async Task OnInitAsync()
    {
        try
        {
            if (!_mutex.WaitOne(1000))
                throw new TimeoutException();

            await _baseStream.WriteAsync(BitConverter.GetBytes(0)).ConfigureAwait(false);

            _baseStream.Seek(6, SeekOrigin.Current);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 用来写入计数的内部方法
    /// </summary>
    /// <returns></returns>
    protected virtual async Task WriteCount(short offset)
    {
        var tmp = _baseStream.Position;
        _baseStream.Seek(_start + 4, SeekOrigin.Begin);
        await _baseStream.ReadAsync(_buffer.AsMemory(0, 2)).ConfigureAwait(false);
        var count = BitConverter.ToInt16(_buffer, 2);
        _baseStream.Seek(-2, SeekOrigin.Current);
        await _baseStream.WriteAsync(BitConverter.GetBytes((short)(count + offset))).ConfigureAwait(false);
        _baseStream.Seek(tmp, SeekOrigin.Begin);
    }
    /// <summary>
    /// 用来写入BodySize的方法
    /// </summary>
    /// <returns></returns>
    public virtual async Task WriteBodySize(int size)
    {
        var tmp = _baseStream.Position;
        _baseStream.Seek(_start + 6, SeekOrigin.Begin);
        await _baseStream.WriteAsync(BitConverter.GetBytes(size)).ConfigureAwait(false);
        _baseStream.Seek(tmp, SeekOrigin.Begin);
    }

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public async Task WriteAsync(MixIndexEntry entry)
    {
        try
        {
            if (!_mutex.WaitOne(1000))
                throw new TimeoutException();

            unsafe
            {
                fixed (byte* ptr = _buffer)
                {
                    MixIndexEntry* p = &entry;
                    Buffer.MemoryCopy(p, ptr, sizeof(MixIndexEntry), sizeof(MixIndexEntry));
                }
            }
            await _baseStream.WriteAsync(_buffer).ConfigureAwait(false);
            await WriteCount(1).ConfigureAwait(false);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                if (!_leaveOpen)
                    _baseStream.Dispose();
                _mutex.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~MixEntryWriter()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
