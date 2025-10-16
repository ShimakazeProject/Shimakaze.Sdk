namespace Shimakaze.Sdk;

/// <summary>
/// 内存实用工具
/// </summary>
internal static class MemoryExtensions
{
#if NETSTANDARD2_0
    public static ReadOnlySpan<T> TrimStart<T>(this ReadOnlySpan<T> span, T trimElement) where T : IEquatable<T>?
    {
        if (trimElement is null)
            throw new ArgumentNullException(nameof(trimElement));

        for (int i = 0; i < span.Length; i++)
        {
            if (!trimElement.Equals(span[i]))
                return span[i..];
        }

        return span[..0];
    }

    public static ReadOnlySpan<T> TrimEnd<T>(this ReadOnlySpan<T> span, T trimElement) where T : IEquatable<T>?
    {
        if (trimElement is null)
            throw new ArgumentNullException(nameof(trimElement));


        for (int i = span.Length - 1; i >= 0; i--)
        {
            if (!trimElement.Equals(span[i]))
                return span[..i];
        }

        return span[..0];
    }
#endif
}
