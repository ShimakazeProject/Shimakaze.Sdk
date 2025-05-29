namespace Shimakaze.Sdk.Common;

/// <summary>
/// 可释放对象
/// </summary>
/// <typeparam name="TDisposable"></typeparam>
/// <param name="disposable"></param>
/// <param name="leaveOpen"></param>
public sealed class DisposableObject<TDisposable>(TDisposable disposable, bool leaveOpen = false) : IDisposable, IAsyncDisposable
    where TDisposable : IDisposable
{
    /// <summary>
    /// 可释放的资源
    /// </summary>
    public TDisposable Resource => disposable;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!leaveOpen)
        {
            disposable.Dispose();
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (disposable is IAsyncDisposable asyncDisposable)
        {
            if (!leaveOpen)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
        }
        else
        {
            if (!leaveOpen)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// 隐式转换可释放对象
    /// </summary>
    /// <param name="disposableObject"></param>
    public static implicit operator TDisposable(DisposableObject<TDisposable> disposableObject)
    {
        return disposableObject.Resource;
    }
}
