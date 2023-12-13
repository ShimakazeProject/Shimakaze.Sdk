namespace Shimakaze.Sdk;

#if NETSTANDARD2_0
public static class StringExtensions
{
    public static string[] Split(this string value, string split)
    {
        List<string> strings = [];

        while (value.IndexOf(split, 0, split.Length, StringComparison.Ordinal) is int index and not -1)
        {
            strings.Add(value[..index]);
            value = value[(index + split.Length)..];
        }

        return [.. strings];
    }
}
#endif