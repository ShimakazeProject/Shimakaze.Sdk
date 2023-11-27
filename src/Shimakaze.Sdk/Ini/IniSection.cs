using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI节
/// </summary>
/// <remarks>
/// 构造一个INISection
/// </remarks>
/// <param name="name">节名</param>
/// <param name="map">节数据 (键值对字典)</param>
public class IniSection(string name, Dictionary<string, string> map) : IDictionary<string, string>
{
    /// <summary>
    /// 内部的INI键值对字典
    /// </summary>
    protected readonly Dictionary<string, string> _map = map;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public int Count => ((ICollection<KeyValuePair<string, string>>)_map).Count;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_map).IsReadOnly;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public ICollection<string> Keys => ((IDictionary<string, string>)_map).Keys;

    /// <summary>
    /// Section节名
    /// </summary>
    public string Name { get; internal set; } = name;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public ICollection<string> Values => ((IDictionary<string, string>)_map).Values;


    /// <inheritdoc cref="IniSection(string, Dictionary{string, string})" />
    public IniSection()
        : this(string.Empty, [])
    { }

    /// <inheritdoc cref="IniSection(string, Dictionary{string, string})" />
    public IniSection(string name)
        : this(name, [])
    { }

    /// <inheritdoc cref="IniSection(string, Dictionary{string, string})" />
    public IniSection(Dictionary<string, string> map)
        : this(string.Empty, map)
    { }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public string this[string key]
    {
        get => ((IDictionary<string, string>)_map)[key];
        set => ((IDictionary<string, string>)_map)[key] = value;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Add(string key, string value)
    {
        ((IDictionary<string, string>)_map).Add(key, value);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Add(KeyValuePair<string, string> item)
    {
        ((ICollection<KeyValuePair<string, string>>)_map).Add(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Clear()
    {
        ((ICollection<KeyValuePair<string, string>>)_map).Clear();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Contains(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>)_map).Contains(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, string>)_map).ContainsKey(key);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, string>>)_map).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, string>>)_map).GetEnumerator();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_map).GetEnumerator();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Remove(string key)
    {
        return ((IDictionary<string, string>)_map).Remove(key);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Remove(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>)_map).Remove(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return ((IDictionary<string, string>)_map).TryGetValue(key, out value);
    }
}