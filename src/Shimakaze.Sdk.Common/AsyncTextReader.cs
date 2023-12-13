namespace Shimakaze.Sdk;

/// <summary>
/// 异步读取器
/// </summary>
/// <typeparam name="T"> </typeparam>
public abstract class AsyncTextReader<T> : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Leave Open
    /// </summary>
    private readonly bool _leaveOpen;

    private bool _disposedValue;

    /// <summary>
    /// 基础流
    /// </summary>
    public TextReader BaseReader { get; }

    /// <summary>
    /// 构造 Csf 读取器
    /// </summary>
    /// <param name="baseReader"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    protected AsyncTextReader(TextReader baseReader, bool leaveOpen = false)
    {
        BaseReader = baseReader;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="ReadAsync(IProgress{float}?, CancellationToken)" />
    public virtual Task<T> ReadAsync() => ReadAsync(default, default);

    /// <inheritdoc cref="ReadAsync(IProgress{float}?, CancellationToken)" />
    public virtual Task<T> ReadAsync(CancellationToken cancellationToken) => ReadAsync(default, default);

    /// <inheritdoc cref="ReadAsync(IProgress{float}?, CancellationToken)" />
    public virtual Task<T> ReadAsync(IProgress<float>? progress) => ReadAsync(default, default);

    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="progress"> 进度 </param>
    /// <param name="cancellationToken"> 取消 </param>
    /// <returns> </returns>
    public abstract Task<T> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// 释放模式
    /// </summary>
    /// <param name="disposing"> </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            if (!_leaveOpen)
                BaseReader.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>
    /// 释放模式
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await Task.Run(() =>
        {
            if (!_leaveOpen)
                BaseReader.Dispose();
        });
    }

    /// <summary>
    /// 终结器
    /// </summary>
    ~AsyncTextReader()
    {
        Dispose(disposing: false);
    }
}
