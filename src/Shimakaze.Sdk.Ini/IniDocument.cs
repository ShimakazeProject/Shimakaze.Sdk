
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI文档
/// </summary>
public class IniDocument : ICollection<IniSection>, IDictionary<string, IniSection>
{
    /// <summary>
    /// 内部的用于快速查询的Section字典
    /// </summary>
    protected Dictionary<string, IniSection> _map = new();

    /// <summary>
    /// 在文档头部的没有包含在Section中的孤立的键值对
    /// </summary>
    public IniSection Default = new() { Name = ";Default;" };

    /// <summary>
    /// 构造一个INI文档
    /// </summary>
    public IniDocument()
    {
    }

    /// <inheritdoc cref="IniDocument.IniDocument()"/>
    /// <param name="map">节列表</param>
    public IniDocument(IEnumerable<IniSection> map) : this()
    {
        _map = map.ToDictionary(i => i.Name);
    }

    /// <inheritdoc/>
    public IniSection this[string key]
    {
        get => ((IDictionary<string, IniSection>)_map)[key];
        set => ((IDictionary<string, IniSection>)_map)[key] = value;
    }

    /// <inheritdoc/>
    public int Count => _map.Count;

    /// <inheritdoc/>
    public bool IsReadOnly { get; } = false;

    /// <inheritdoc/>
    public ICollection<string> Keys => ((IDictionary<string, IniSection>)_map).Keys;

    /// <inheritdoc/>
    public ICollection<IniSection> Values => ((IDictionary<string, IniSection>)_map).Values;

    /// <inheritdoc/>
    public void Add(IniSection item)
    {
        Add(item.Name, item);
    }

    /// <inheritdoc/>
    public void Add(string key, IniSection value)
    {
        ((IDictionary<string, IniSection>)_map).Add(key, value);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, IniSection> item)
    {
        ((ICollection<KeyValuePair<string, IniSection>>)_map).Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _map.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(IniSection item)
    {
        return ContainsKey(item.Name);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, IniSection> item)
    {
        return ((ICollection<KeyValuePair<string, IniSection>>)_map).Contains(item);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, IniSection>)_map).ContainsKey(key);
    }

    /// <inheritdoc/>
    public void CopyTo(IniSection[] array, int arrayIndex)
    {
        Values.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, IniSection>>)_map).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<IniSection> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    /// <inheritdoc/>
    public bool Remove(IniSection item)
    {
        return Remove(item.Name);
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        return ((IDictionary<string, IniSection>)_map).Remove(key);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, IniSection> item)
    {
        return ((ICollection<KeyValuePair<string, IniSection>>)_map).Remove(item);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IniSection value)
    {
        return ((IDictionary<string, IniSection>)_map).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, IniSection>> IEnumerable<KeyValuePair<string, IniSection>>.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, IniSection>>)_map).GetEnumerator();
    }
}