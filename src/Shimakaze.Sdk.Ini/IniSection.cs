
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI节
/// </summary>
public class IniSection : IDictionary<string, string>
{
    /// <summary>
    /// 内部的INI键值对字典
    /// </summary>
    protected readonly Dictionary<string, string> _map = new();

    /// <summary>
    /// Section节名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 构造一个INISection
    /// </summary>
    public IniSection()
    {
    }

    /// <inheritdoc cref="IniSection.IniSection()"/>
    /// <param name="map">键值对字典</param>
    public IniSection(Dictionary<string, string> map) : this()
    {
        _map = map;
    }


    /// <inheritdoc/>
    public string this[string key]
    {
        get => ((IDictionary<string, string>)_map)[key];
        set => ((IDictionary<string, string>)_map)[key] = value;
    }

    /// <inheritdoc/>
    public ICollection<string> Keys => ((IDictionary<string, string>)_map).Keys;

    /// <inheritdoc/>
    public ICollection<string> Values => ((IDictionary<string, string>)_map).Values;

    /// <inheritdoc/>
    public int Count => ((ICollection<KeyValuePair<string, string>>)_map).Count;

    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_map).IsReadOnly;

    /// <inheritdoc/>
    public void Add(string key, string value)
    {
        ((IDictionary<string, string>)_map).Add(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, string> item)
    {
        ((ICollection<KeyValuePair<string, string>>)_map).Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ((ICollection<KeyValuePair<string, string>>)_map).Clear();
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>)_map).Contains(item);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, string>)_map).ContainsKey(key);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, string>>)_map).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, string>>)_map).GetEnumerator();
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        return ((IDictionary<string, string>)_map).Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>)_map).Remove(item);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return ((IDictionary<string, string>)_map).TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_map).GetEnumerator();
    }
}