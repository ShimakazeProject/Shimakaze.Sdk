namespace Shimakaze.Sdk;

/// <summary>
/// 垫片
/// </summary>
public static class StringShim
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strings"></param>
    /// <param name="ch"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<string> strings, char ch)
    {
#if NETSTANDARD2_0
        return string.Join(new(ch, 1), strings);
#else
        return string.Join(ch, strings);
#endif
    }
}