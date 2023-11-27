using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI文档
/// </summary>
public sealed class IniDocument() : ICollection<IniSection>, IDictionary<string, IniSection>
{
    /// <summary>
    /// 在文档头部的没有包含在Section中的孤立的键值对
    /// </summary>
    public IniSection Default = new() { Name = ";Default;" };

    /// <summary>
    /// 内部的用于快速查询的Section字典
    /// </summary>
    private readonly Dictionary<string, IniSection> _data = [];

    /// <inheritdoc/>
    public int Count => _data.Count;

    bool ICollection<KeyValuePair<string, IniSection>>.IsReadOnly => ((ICollection<KeyValuePair<string, IniSection>>)_data).IsReadOnly;
    bool ICollection<IniSection>.IsReadOnly => ((ICollection<KeyValuePair<string, IniSection>>)_data).IsReadOnly;

    /// <inheritdoc/>
    public ICollection<string> Keys => _data.Keys;

    /// <inheritdoc/>
    public ICollection<IniSection> Values => _data.Values;

    /// <inheritdoc/>
    public IniSection this[string key]
    {
        get => _data[key];
        set => _data[key] = value;
    }

    /// <summary>
    /// 构造一个INI文档
    /// </summary>
    /// <param name="sections"> 节列表 </param>
    public IniDocument(IEnumerable<IniSection> sections) : this() => _data = sections.ToDictionary(i => i.Name);

    /// <inheritdoc/>
    public void Add(IniSection item) => _data.Add(item.Name, item);
    void IDictionary<string, IniSection>.Add(string key, IniSection value) => Add(value);
    void ICollection<KeyValuePair<string, IniSection>>.Add(KeyValuePair<string, IniSection> item) => Add(item.Value);

    /// <inheritdoc/>
    public void Clear() => _data.Clear();

    /// <summary>
    /// ContainsSection
    /// </summary>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public bool ContainsSection(string sectionName) => _data.ContainsKey(sectionName);

    /// <inheritdoc/>
    [Obsolete("Use ContainsSection instead of.")]
    public bool ContainsKey(string key) => ContainsSection(key);
    bool ICollection<IniSection>.Contains(IniSection item) => ContainsSection(item.Name);
    bool ICollection<KeyValuePair<string, IniSection>>.Contains(KeyValuePair<string, IniSection> item) => ContainsSection(item.Key);

    /// <inheritdoc/>
    public bool Remove(string key) => _data.Remove(key);
    bool ICollection<IniSection>.Remove(IniSection item) => Remove(item.Name);
    bool ICollection<KeyValuePair<string, IniSection>>.Remove(KeyValuePair<string, IniSection> item) => Remove(item.Key);

    /// <summary>
    /// TryGetSection
    /// </summary>
    /// <param name="key"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    public bool TryGetSection(string key, [MaybeNullWhen(false)] out IniSection section) => _data.TryGetValue(key, out section);
    /// <inheritdoc/>
    [Obsolete("Use TryGetSection instead of.")]
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IniSection value) => _data.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IEnumerator<IniSection> GetEnumerator() => _data.Values.GetEnumerator();

    IEnumerator<KeyValuePair<string, IniSection>> IEnumerable<KeyValuePair<string, IniSection>>.GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<IniSection>.CopyTo(IniSection[] array, int arrayIndex) => Values.CopyTo(array, arrayIndex);
    void ICollection<KeyValuePair<string, IniSection>>.CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, IniSection>>)_data).CopyTo(array, arrayIndex);

}