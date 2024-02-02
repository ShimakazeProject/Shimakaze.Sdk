using System;
using System.IO;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix Entry 读取器
/// </summary>
public sealed class MixEntryWriter(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream.CanSeek(), leaveOpen);

    /// <summary>
    /// 当前文件个数
    /// </summary>
    private short _count;

    private bool _inited;

    /// <summary>
    /// 当前文件大小
    /// </summary>
    private int _size;

    /// <summary>
    /// 流开始的位置
    /// </summary>
    private long _start;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        _start = _disposable.Resource.Position;
        _disposable.Resource.Seek(4 + 2 + 4, SeekOrigin.Current);

        _inited = true;
    }

    /// <inheritdoc />
    public void Write(in MixEntry value)
    {
        if (!_inited)
            Init();

        _disposable.Resource.Write(value);

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
    }

    /// <summary>
    /// 写入元数据
    /// </summary>
    public void WriteMetadata()
    {
        long current = _disposable.Resource.Position;
        _disposable.Resource.Seek(_start, SeekOrigin.Begin);
        WriteMetadataDirect((int)MixTag.NONE, new(_count, _size));
        _disposable.Resource.Seek(current, SeekOrigin.Begin);
    }

    /// <summary>
    /// 直接写入元数据
    /// </summary>
    /// <param name="flag"> 标记 </param>
    /// <param name="metadata"> 元数据 </param>
    internal void WriteMetadataDirect(int flag, MixMetadata metadata)
    {
        _disposable.Resource.Write(BitConverter.GetBytes(flag));
        _disposable.Resource.Write(metadata);
    }

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

}