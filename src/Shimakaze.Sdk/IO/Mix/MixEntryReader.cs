using Shimakaze.Sdk.Data.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix文件头读取器
/// </summary>
public class MixEntryReader : IDisposable
{
    /// <summary>
    /// Mix文件流
    /// </summary>
    protected Stream _baseStream;
    private readonly byte[] _buffer = new byte[12];
    private readonly bool _leaveOpen;
    /// <summary>
    /// Entry的数量
    /// </summary>
    public short Count { get; private set; }
    /// <summary>
    /// 主体部分文件大小
    /// </summary>
    public int BodySize { get; private set; }
    /// <summary>
    /// 主体部分偏移位置
    /// </summary>
    public long BodyOffset { get; private set; }
    /// <summary>
    /// 线程安全锁
    /// </summary>
    protected Mutex _mutex = new();
    private bool _disposedValue;

    /// <summary>
    /// 构造一个读取器
    /// </summary>
    /// <param name="stream">Mix文件流</param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MixEntryReader(Stream stream, bool leaveOpen)
    {
        _baseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        _ = OnInitAsync();
        _leaveOpen = leaveOpen;
    }
    /// <summary>
    /// 初始化方法
    /// </summary>
    /// <returns></returns>
    /// <exception cref="TimeoutException">初始化异常</exception>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual async Task OnInitAsync()
    {
        try
        {
            if (!_mutex.WaitOne(1000))
                throw new TimeoutException();

            // 标识符
            await _baseStream.ReadAsync(_buffer.AsMemory(0, 10)).ConfigureAwait(false);
            MixFileFlag flag = (MixFileFlag)BitConverter.ToUInt32(_buffer, 0);
            if ((flag & MixFileFlag.ENCRYPTED) is not 0)
                throw new NotImplementedException("This Mix File is Encrypted.");

            Count = BitConverter.ToInt16(_buffer, 4);
            BodySize = BitConverter.ToInt32(_buffer, 6);
            BodyOffset = _baseStream.Position + 12 * Count;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// 读出下一个Entry
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EndOfStreamException">没有可供读取的Entry了</exception>
    public async Task<MixIndexEntry> ReadAsync()
    {
        try
        {
            if (!_mutex.WaitOne(1000))
                throw new TimeoutException();

            if (_baseStream.Position >= BodyOffset)
                throw new EndOfStreamException();

            await _baseStream.ReadAsync(_buffer);
            unsafe
            {
                fixed (byte* ptr = _buffer)
                    return *(MixIndexEntry*)ptr;
            }
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
    // ~MixEntryReader()
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
