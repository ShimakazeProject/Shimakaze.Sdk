using System.Collections;

namespace Shimakaze.Sdk.Utils;

public static class EachExtension
{
    public static void Each<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T item in source)
            action(item);
    }

    public static void Each<T>(this IEnumerable source, Action<T> action)
    {
        foreach (T item in source)
        {
            action(item);
        }
    }

    public static IEnumerable<TResult> Each<T, TResult>(this IEnumerable source, Func<T, TResult> func)
    {
        List<TResult> result = new();
        foreach (T item in source)
        {
            result.Add(func(item));
        }

        return result;
    }

    /// <summary>
    /// ForEach Collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts"></param>
    /// <param name="action">Break the loop when this delegate result is false</param>
    public static void Each<T>(this IEnumerable<T> ts, Func<T, bool> action)
    {
        foreach (T? item in ts)
        {
            if (!action(item))
            {
                break;
            }
        }
    }

    public static async Task EachAsync<T>(this IEnumerable<T> ts, Func<T, Task> action)
    {
        foreach (T? item in ts)
        {
            await action(item);
        }
    }

    /// <summary>
    /// ForEach Collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts"></param>
    /// <param name="action">Break the loop when this delegate result is false</param>
    public static async Task EachAsync<T>(this IEnumerable<T> ts, Func<T, Task<bool>> action)
    {
        foreach (T? item in ts)
        {
            if (!await action(item))
            {
                break;
            }
        }
    }
}
