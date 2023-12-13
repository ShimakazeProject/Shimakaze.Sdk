#if NETSTANDARD

namespace System.Linq;

public static class LinqExtensions
{
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => DistinctBy(source, keySelector, null);

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (keySelector is null)
            throw new ArgumentNullException(nameof(keySelector));

        Dictionary<TKey, TSource> tmp = new(comparer);

        foreach (var item in source)
            tmp.TryAdd(keySelector(item), item);

        return tmp.Values;
    }
}
#endif