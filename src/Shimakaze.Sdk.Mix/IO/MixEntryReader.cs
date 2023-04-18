using System.Runtime.InteropServices;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;
/// <summary>
/// Mix Entry 读取器
/// </summary>
public class MixEntryReader : IReader<MixEntry>, IAsyncReader<Task<MixEntry>>, IDisposable, IAsyncDisposable
{
    private bool _inited;
    private bool _disposedValue;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer;

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }
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
    /// 构造 Mix Entry 读取器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    /// <param name="buffer">缓冲区大小</param>
    public MixEntryReader(Stream baseStream, bool leaveOpen = false, byte[]? buffer = default)
    {
        _buffer = buffer ?? new byte[128];
        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <exception cref="NotImplementedException">当Mix Entry被加密时抛出</exception>
    public virtual void Init()
    {
        // 标识符
        BaseStream.Read(_buffer.AsSpan(0, 4));
        MixFlag flag = (MixFlag)BitConverter.ToUInt32(_buffer, 0);
        if ((flag & MixFlag.ENCRYPTED) is not 0)
            throw new NotImplementedException("This Mix File is Encrypted.");

        BaseStream.Read(_buffer.AsSpan(0, 6));

        MixMetadata info;
        unsafe
        {
            fixed (byte* ptr = _buffer)
                info = *(MixMetadata*)ptr;
        }

        Count = info.Files;
        BodySize = info.Size;
        BodyOffset = BaseStream.Position + 12 * Count;

        _inited = true;
    }

    /// <inheritdoc cref="Init"/>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>可等待的任务</returns>
    /// <exception cref="NotImplementedException">当Mix Entry被加密时抛出</exception>
    public virtual async Task InitAsync(CancellationToken cancellationToken = default)
    {
        // 标识符
        await BaseStream.ReadAsync(_buffer.AsMemory(0, 4), cancellationToken);
        MixFlag flag = (MixFlag)BitConverter.ToUInt32(_buffer, 0);
        if ((flag & MixFlag.ENCRYPTED) is not 0)
            throw new NotImplementedException("This Mix File is Encrypted.");

        await BaseStream.ReadAsync(_buffer.AsMemory(0, 6), cancellationToken);

        MixMetadata info;
        unsafe
        {
            fixed (byte* ptr = _buffer)
                info = *(MixMetadata*)ptr;
        }

        Count = info.Files;
        BodySize = info.Size;
        BodyOffset = BaseStream.Position + 12 * Count;

        _inited = true;
    }

    /// <summary>
    /// 读取一个Entry
    /// </summary>
    /// <returns>Entry</returns>
    /// <exception cref="EndOfEntryTableException">当没有可被读取的Entry时抛出</exception>
    public virtual MixEntry Read()
    {
        if (!_inited)
            Init();

        if (BaseStream.Position >= BodyOffset)
            throw new EndOfEntryTableException();

        BaseStream.Read(_buffer.AsSpan(0, Marshal.SizeOf<MixEntry>()));
        unsafe
        {
            fixed (byte* ptr = _buffer)
                return *(MixEntry*)ptr;
        }
    }

    /// <inheritdoc cref="Read"/>
    /// <inheritdoc cref="InitAsync"/>
    public virtual async Task<MixEntry> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!_inited)
            await InitAsync(cancellationToken);

        if (BaseStream.Position >= BodyOffset)
            throw new EndOfEntryTableException();

        await BaseStream.ReadAsync(_buffer.AsMemory(0, Marshal.SizeOf<MixEntry>()), cancellationToken);
        unsafe
        {
            fixed (byte* ptr = _buffer)
                return *(MixEntry*)ptr;
        }
    }


    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                    BaseStream.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// 异步释放核心
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }

    // ~CsfReader()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}
