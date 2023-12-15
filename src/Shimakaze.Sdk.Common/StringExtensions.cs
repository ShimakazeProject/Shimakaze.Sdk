namespace Shimakaze.Sdk;

/// <summary>
/// 字符串实用工具
/// </summary>
internal static class StringExtensions
{
    /// <inheritdoc cref="string.Join(string, string[])"/>
    public static string Join(this IEnumerable<string> value, char separator)
    {
#if NETSTANDARD2_0 || NETFRAMEWORK
        return string.Join(new(separator, 1), value);
#else
        return string.Join(separator, value);
#endif
    }
}