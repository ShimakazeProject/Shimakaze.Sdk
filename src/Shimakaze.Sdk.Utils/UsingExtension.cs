namespace Shimakaze.Sdk.Utils;

public static class DisposableExtensions
{
    public static void Using<T>(this T resource, Action<T> action) where T : IDisposable
    {
        using T res = resource;
        action(res);
    }
    public static TResult Using<T, TResult>(this T resource, Func<T, TResult> action) where T : IDisposable
    {
        using T res = resource;
        return action(res);
    }

    public static async Task UsingAsync<T>(this T resource, Func<T, Task> action) where T : IDisposable
    {
        using T res = resource;
        await action(res);
    }
    public static async Task<TResult> Using<T, TResult>(this T resource, Func<T, Task<TResult>> action) where T : IDisposable
    {
        using T res = resource;
        return await action(res);
    }


}

public static class SystemExtensions
{
    public static void Then<T>(this T @this, Action<T> action) => action(@this);
    public static TResult Then<T, TResult>(this T @this, Func<T, TResult> action) => action(@this);

    public static void Then<T1, T2>(this ValueTuple<T1, T2> @this, Action<T1, T2> action) => action(@this.Item1, @this.Item2);
    public static TResult Then<T1, T2, TResult>(this ValueTuple<T1, T2> @this, Func<T1, T2, TResult> action) => action(@this.Item1, @this.Item2);
}