namespace Shimakaze.Sdk;

/// <summary>
/// 可释放对象
/// </summary>
/// <typeparam name="TDisposable"></typeparam>
/// <param name="disposable"></param>
/// <param name="leaveOpen"></param>
internal sealed class DisposableObject<TDisposable>(TDisposable disposable, bool leaveOpen = false) : IDisposable, IAsyncDisposable
    where TDisposable : IDisposable
{
    public TDisposable Resource => disposable;

    public void Dispose()
    {
        if (!leaveOpen)
            disposable.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (disposable is IAsyncDisposable asyncDisposable)
        {
            if (!leaveOpen)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            if (!leaveOpen)
                disposable.Dispose();
        }
    }

    public static implicit operator TDisposable(DisposableObject<TDisposable> disposableObject) => disposableObject.Resource;
}