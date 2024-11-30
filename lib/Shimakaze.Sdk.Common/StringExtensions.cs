namespace Shimakaze.Sdk.Common;

/// <summary>
/// 字符串实用工具
/// </summary>
public static class StringExtensions
{
    /// <inheritdoc cref="string.Join(string, string[])"/>
    public static string Join(this IEnumerable<string> value, char separator)
    {
        return string.Join(separator, value);
    }
}
