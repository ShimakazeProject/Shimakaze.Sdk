using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.IO.Csf.Serialization;

/// <summary>
/// Csf 序列化器
/// </summary>
public class CsfDeserializer : IDeserializer<CsfDocument>, IAsyncDeserializer<Task<CsfDocument>>, IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    /// <summary>
    /// 写入器
    /// </summary>
    protected CsfReader _reader;

    /// <summary>
    /// Csf序列化器 构造器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    /// <exception cref="NotSupportedException">当流不支持Seek时抛出</exception>
    public CsfDeserializer(Stream baseStream, bool leaveOpen = false)
    {
        _reader = new(baseStream, leaveOpen);
    }

    /// <inheritdoc/>
    public virtual CsfDocument Deserialize()
    {
        _reader.Init();
        List<CsfData> lists = new(_reader.Metadata.LabelCount);
        for (int i = 0; i < _reader.Metadata.LabelCount; i++)
            lists.Add(_reader.Read());

        return new(_reader.Metadata, lists);
    }

    /// <inheritdoc/>
    public virtual async Task<CsfDocument> DeserializeAsync(CancellationToken cancellationToken = default)
    {
        await _reader.InitAsync(cancellationToken);
        List<CsfData> lists = new(_reader.Metadata.LabelCount);
        for (int i = 0; i < _reader.Metadata.LabelCount; i++)
            lists.Add(await _reader.ReadAsync(cancellationToken));

        return new(_reader.Metadata, lists);
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
                _reader.Dispose();
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
        await _reader.DisposeAsync();
    }

    // ~CsfDeserializer()
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