namespace Shimakaze.Sdk.Utils;

public static class AsyncDisposableExtensions
{
    public static async Task AwaitUsing<T>(this T resource, Action<T> action) where T : IAsyncDisposable
    {
        await using T res = resource;
        action(res);
    }
    public static async Task<TResult> AwaitUsing<T, TResult>(this T resource, Func<T, TResult> action) where T : IAsyncDisposable
    {
        await using T res = resource;
        return action(res);
    }

    public static async Task AwaitUsing<T>(this T resource, Func<T, Task> action) where T : IAsyncDisposable
    {
        await using T res = resource;
        await action(res);
    }
    public static async Task<TResult> AwaitUsing<T, TResult>(this T resource, Func<T, Task<TResult>> action) where T : IAsyncDisposable
    {
        await using T res = resource;
        return await action(res);
    }
}