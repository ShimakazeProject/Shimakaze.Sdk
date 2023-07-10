using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI文档
/// </summary>
public class IniDocument : ICollection<IniSection>, IDictionary<string, IniSection>
{
    /// <summary>
    /// 在文档头部的没有包含在Section中的孤立的键值对
    /// </summary>
    public IniSection Default = new() { Name = ";Default;" };

    /// <summary>
    /// 内部的用于快速查询的Section字典
    /// </summary>
    protected Dictionary<string, IniSection> _map = new();

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <inheritdoc />
    public bool IsReadOnly { get; } = false;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public ICollection<string> Keys => ((IDictionary<string, IniSection>)_map).Keys;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public ICollection<IniSection> Values => ((IDictionary<string, IniSection>)_map).Values;

    /// <summary>
    /// 构造一个INI文档
    /// </summary>
    public IniDocument()
    {
    }

    /// <inheritdoc cref="IniDocument()" />
    /// <param name="map"> 节列表 </param>
    public IniDocument(IEnumerable<IniSection> map) : this()
    {
        _map = map.ToDictionary(i => i.Name);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public IniSection this[string key]
    {
        get => ((IDictionary<string, IniSection>)_map)[key];
        set => ((IDictionary<string, IniSection>)_map)[key] = value;
    }

    /// <inheritdoc />
    public void Add(IniSection item)
    {
        Add(item.Name, item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Add(string key, IniSection value)
    {
        ((IDictionary<string, IniSection>)_map).Add(key, value);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Add(KeyValuePair<string, IniSection> item)
    {
        ((ICollection<KeyValuePair<string, IniSection>>)_map).Add(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Clear()
    {
        _map.Clear();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Contains(IniSection item)
    {
        return ContainsKey(item.Name);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Contains(KeyValuePair<string, IniSection> item)
    {
        return ((ICollection<KeyValuePair<string, IniSection>>)_map).Contains(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, IniSection>)_map).ContainsKey(key);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void CopyTo(IniSection[] array, int arrayIndex)
    {
        Values.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, IniSection>>)_map).CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public IEnumerator<IniSection> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [ExcludeFromCodeCoverage]
    IEnumerator<KeyValuePair<string, IniSection>> IEnumerable<KeyValuePair<string, IniSection>>.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, IniSection>>)_map).GetEnumerator();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Remove(IniSection item)
    {
        return Remove(item.Name);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Remove(string key)
    {
        return ((IDictionary<string, IniSection>)_map).Remove(key);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Remove(KeyValuePair<string, IniSection> item)
    {
        return ((ICollection<KeyValuePair<string, IniSection>>)_map).Remove(item);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IniSection value)
    {
        return ((IDictionary<string, IniSection>)_map).TryGetValue(key, out value);
    }
}