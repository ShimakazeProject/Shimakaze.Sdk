using System.IO;
using System;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Mix Entry 读取器
/// </summary>
/// <remarks>
/// 构造 Mix Entry 读取器
/// </remarks>
/// <param name="stream"> 基础流 </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class MixEntryReader(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private bool _inited;

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
    /// 初始化
    /// </summary>
    /// <exception cref="NotImplementedException"> 当Mix Entry被加密时抛出 </exception>
    public void Init()
    {
        // 标识符
        _disposable.Resource.Read(out MixTag flag);
        if ((flag & MixTag.ENCRYPTED) is not 0)
            throw new NotImplementedException("This Mix File is Encrypted.");

        _disposable.Resource.Read(out MixMetadata info);

        Count = info.Files;
        BodySize = info.Size;
        BodyOffset = _disposable.Resource.Position + 12 * Count;

        _inited = true;
    }

    /// <summary>
    /// 读取一个Entry
    /// </summary>
    /// <returns> Entry </returns>
    /// <exception cref="EndOfEntryTableException"> 当没有可被读取的Entry时抛出 </exception>
    public MixEntry Read()
    {
        if (!_inited)
            Init();

        if (_disposable.Resource.Position >= BodyOffset)
            throw new EndOfEntryTableException();

        _disposable.Resource.Read(out MixEntry entry);
        return entry;
    }

    /// <summary>
    /// 读取所有的Entry
    /// </summary>
    /// <returns></returns>
    public MixEntry[] ReadAll()
    {
        if (!_inited)
            Init();

        MixEntry[]  entries = new MixEntry[Count];
        for (int i = 0; i < Count; i++)
            _disposable.Resource.Read(out entries[i]);

        return entries;
    }

    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();
}
