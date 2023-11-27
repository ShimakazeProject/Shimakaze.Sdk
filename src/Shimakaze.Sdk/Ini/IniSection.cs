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
public sealed class IniSection(string name, Dictionary<string, string> map) : IDictionary<string, string>
{
    /// <summary>
    /// 内部的INI键值对字典
    /// </summary>
    private readonly Dictionary<string, string> _data = map;

    /// <summary>
    /// Section节名
    /// </summary>
    public string Name { get; internal set; } = name;

    /// <inheritdoc/>
    public ICollection<string> Keys => _data.Keys;

    /// <inheritdoc/>
    public ICollection<string> Values => _data.Values;

    /// <inheritdoc/>
    public int Count => _data.Count;

    bool ICollection<KeyValuePair<string, string>>.IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_data).IsReadOnly;

    /// <inheritdoc/>
    public string this[string key] { get => _data[key]; set => _data[key] = value; }

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

    /// <inheritdoc/>
    public void Add(string key, string value) => _data.Add(key, value);

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <inheritdoc/>
    public bool Remove(string key) => _data.Remove(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _data.TryGetValue(key, out value);

    /// <inheritdoc/>
    public void Clear() => _data.Clear();

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, string> item) => _data.Contains(item);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_data).Add(item);

    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)_data).CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_data).Remove(item);
}