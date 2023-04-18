using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.IO.Csf.Serialization;

/// <summary>
/// Csf 序列化器
/// </summary>
public class CsfSerializer : ISerializer<CsfDocument>, IAsyncSerializer<CsfDocument, Task>, IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    /// <summary>
    /// 写入器
    /// </summary>
    protected CsfWriter _writer;

    /// <summary>
    /// Csf序列化器 构造器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    /// <exception cref="NotSupportedException">当流不支持Seek时抛出</exception>
    public CsfSerializer(Stream baseStream, bool leaveOpen = false)
    {
        _writer = new(baseStream, leaveOpen);
    }

    /// <inheritdoc/>
    public virtual void Serialize(CsfDocument value)
    {
        _writer.WriteMetadataDirect(value.Metadata);
        foreach (var item in value.Data)
            _writer.Write(item);
    }

    /// <inheritdoc/>
    public virtual async Task SerializeAsync(CsfDocument value, CancellationToken cancellationToken = default)
    {
        _writer.WriteMetadataDirect(value.Metadata);
        foreach (var item in value.Data)
            await _writer.WriteAsync(item, cancellationToken);
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
                _writer.Dispose();
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
        await _writer.DisposeAsync();
    }

    // ~CsfSerializer()
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