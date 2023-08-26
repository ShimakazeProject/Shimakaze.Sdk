using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Common;

/// <summary>
/// 异步写入器
/// </summary>
/// <typeparam name="T"> </typeparam>
[ExcludeFromCodeCoverage]
public abstract class AsyncWriter<T> : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Leave Open
    /// </summary>
    protected readonly bool _leaveOpen;

    private bool _disposedValue;

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// 构造 Csf 读取器
    /// </summary>
    /// <param name="baseStream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    protected AsyncWriter(Stream baseStream, bool leaveOpen = false)
    {
        BaseStream = baseStream;
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

    /// <inheritdoc cref="WriteAsync(T, IProgress{float}?, CancellationToken)" />
    public virtual Task WriteAsync(T value) => WriteAsync(value, default, default);

    /// <inheritdoc cref="WriteAsync(T, IProgress{float}?, CancellationToken)" />
    public virtual Task WriteAsync(T value, CancellationToken cancellationToken) => WriteAsync(value, default, default);

    /// <inheritdoc cref="WriteAsync(T, IProgress{float}?, CancellationToken)" />
    public virtual Task WriteAsync(T value, IProgress<float>? progress) => WriteAsync(value, default, default);

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="value"> 值 </param>
    /// <param name="progress"> 进度 </param>
    /// <param name="cancellationToken"> 取消 </param>
    /// <returns> </returns>
    public abstract Task WriteAsync(T value, IProgress<float>? progress = default, CancellationToken cancellationToken = default);

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
                BaseStream.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>
    /// 释放模式
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 终结器
    /// </summary>
    ~AsyncWriter()
    {
        Dispose(disposing: false);
    }
}