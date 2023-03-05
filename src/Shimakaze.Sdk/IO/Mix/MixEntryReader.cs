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
    private bool _disposedValue;

    /// <summary>
    /// 构造一个读取器
    /// </summary>
    /// <param name="stream">Mix文件流</param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected MixEntryReader(Stream stream, bool leaveOpen = false)
    {
        _baseStream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// 构造一个读取器
    /// </summary>
    /// <param name="stream">Mix文件流</param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task<MixEntryReader> CreateAsync(Stream stream, bool leaveOpen = false)
    {
        MixEntryReader reader = new(stream, leaveOpen);
        await reader.OnInitAsync();
        return reader;
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual async Task OnInitAsync()
    {
        // 标识符
        await _baseStream.ReadAsync(_buffer.AsMemory(0, 4)).ConfigureAwait(false);
        MixFlag flag = (MixFlag)BitConverter.ToUInt32(_buffer, 0);
        if ((flag & MixFlag.ENCRYPTED) is not 0)
            throw new NotImplementedException("This Mix File is Encrypted.");

        await _baseStream.ReadAsync(_buffer.AsMemory(0, 6)).ConfigureAwait(false);

        MixMetadata info;
        unsafe
        {
            fixed (byte* ptr = _buffer)
                info = *(MixMetadata*)ptr;
        }

        Count = info.Files;
        BodySize = info.Size;
        BodyOffset = _baseStream.Position + 12 * Count;
    }

    /// <summary>
    /// 读出下一个Entry
    /// </summary>
    /// <returns></returns>
    /// <exception cref="EndOfStreamException">没有可供读取的Entry了</exception>
    public async Task<MixEntry> ReadAsync()
    {
        if (_baseStream.Position >= BodyOffset)
            throw new EndOfStreamException();

        await _baseStream.ReadAsync(_buffer);
        unsafe
        {
            fixed (byte* ptr = _buffer)
                return *(MixEntry*)ptr;
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
                if (!_leaveOpen)
                    _baseStream.Dispose();
            }

            _disposedValue = true;
        }
    }

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
