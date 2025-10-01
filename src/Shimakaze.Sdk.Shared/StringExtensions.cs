namespace Shimakaze.Sdk;

/// <summary>
/// 字符串实用工具
/// </summary>
internal static class StringExtensions
{
    /// <inheritdoc cref="string.Join(string, IEnumerable{string})"/>
    public static string Join(this IEnumerable<string> value, string separator) => string.Join(separator, value);

    /// <inheritdoc cref="Join(IEnumerable{string}, string)"/>
    public static string Join(this IEnumerable<string> value, char separator)
    {
#if NETSTANDARD2_0
        return string.Join(separator.ToString(), value);
#else
        return string.Join(separator, value);
#endif
    }
}
