using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;
/// <summary>
/// Mix Entry 读取器
/// </summary>
public class MixEntryWriter : IWriter<MixEntry>, IAsyncWriter<MixEntry, Task>, IDisposable, IAsyncDisposable
{
    private bool _inited;
    private bool _disposedValue;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer;

    /// <summary>
    /// 当前文件个数
    /// </summary>
    protected short _count;
    /// <summary>
    /// 当前文件大小
    /// </summary>
    protected int _size;
    /// <summary>
    /// 流开始的位置
    /// </summary>
    protected long _start;

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// 构造 Mix Entry 读取器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    /// <param name="buffer">缓冲区大小</param>
    public MixEntryWriter(Stream baseStream, bool leaveOpen = false, byte[]? buffer = default)
    {
        _buffer = buffer ?? new byte[12];

        if (!baseStream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");

        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        _start = BaseStream.Position;
        BaseStream.Seek(4 + 2 + 4, SeekOrigin.Current);

        _inited = true;
    }

    /// <inheritdoc/>
    public virtual void Write(MixEntry value)
    {
        if (!_inited)
            Init();

        unsafe
        {
            fixed (byte* ptr = _buffer)
            {
                MixEntry* p = &value;
                Buffer.MemoryCopy(p, ptr, sizeof(MixEntry), sizeof(MixEntry));
            }
        }
        BaseStream.Write(_buffer.AsSpan(0, 12));

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync(MixEntry value, CancellationToken cancellationToken = default)
    {
        if (!_inited)
            Init();

        unsafe
        {
            fixed (byte* ptr = _buffer)
            {
                MixEntry* p = &value;
                Buffer.MemoryCopy(p, ptr, sizeof(MixEntry), sizeof(MixEntry));
            }
        }
        await BaseStream.WriteAsync(_buffer.AsMemory(0, 12), cancellationToken);

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
    }


    /// <summary>
    /// 写入元数据
    /// </summary>
    public virtual void WriteMetadata()
    {
        long current = BaseStream.Position;
        BaseStream.Seek(_start, SeekOrigin.Begin);
        WriteMetadataDirect((int)MixFlag.NONE, new(_count, _size));
        BaseStream.Seek(current, SeekOrigin.Begin);
    }

    /// <summary>
    /// 直接写入元数据
    /// </summary>
    /// <param name="flag">标记</param>
    /// <param name="metadata">元数据</param>
    internal protected virtual void WriteMetadataDirect(int flag, MixMetadata metadata)
    {
        BaseStream.Write(BitConverter.GetBytes(flag));
        unsafe
        {
            nint ptr = Marshal.AllocHGlobal(sizeof(MixMetadata));
            try
            {
                Marshal.StructureToPtr(metadata, ptr, true);
                BaseStream.Write(new Span<byte>((byte*)ptr, sizeof(MixMetadata)));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
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
