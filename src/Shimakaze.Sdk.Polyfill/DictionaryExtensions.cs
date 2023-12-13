#if NETSTANDARD2_0

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue value)
    {
        if (@this.ContainsKey(key))
        {
            @this[key] = value;
            return true;
        }
        else
        {
            return false;
        }
    }
}
#endif