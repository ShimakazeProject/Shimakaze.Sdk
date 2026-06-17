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

#if NETSTANDARD
    public readonly record struct StringSplitOptionsPolyfill(int Value)
    {
        public static readonly StringSplitOptionsPolyfill TrimEntries = new(2);
        public static implicit operator StringSplitOptionsPolyfill(StringSplitOptions options) => new((int)options);
    }

    extension(StringSplitOptions options)
    {
        public static StringSplitOptionsPolyfill TrimEntries => StringSplitOptionsPolyfill.TrimEntries;
    }

    extension(string str)
    {
        public string[] Split(char separator, StringSplitOptionsPolyfill options)
        {
            string[] result = str.Split(separator, (StringSplitOptions)options.Value);
            return (options.Value & 2) is not 0
                ? [.. result.Select(x => x.Trim())]
                : result;
        }
    }
#endif
}
