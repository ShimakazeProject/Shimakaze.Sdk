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
#pragma warning disable CA1710 // 标识符应具有正确的后缀
public class IniSection(string name, Dictionary<string, string> map) : IIniSection, IDictionary<string, string>
#pragma warning restore CA1710 // 标识符应具有正确的后缀
{
    /// <summary>
    /// 内部的INI键值对字典
    /// </summary>
    protected Dictionary<string, string> Data { get; } = map;

    /// <summary>
    /// Section节名
    /// </summary>
    public virtual string Name { get; internal set; } = name;

    /// <inheritdoc/>
    public virtual ICollection<string> Keys => Data.Keys;

    /// <inheritdoc/>
    public virtual ICollection<string> Values => Data.Values;

    /// <inheritdoc/>
    public virtual int Count => Data.Count;


    /// <inheritdoc/>
    public virtual string this[string key] { get => Data[key]; set => Data[key] = value; }

    /// <inheritdoc cref="IniSection(string, Dictionary{string, string})" />
    public IniSection(string name)
        : this(name, [])
    { }

    /// <inheritdoc/>
    public virtual void Add(string key, string value) => Data.Add(key, value);

    /// <inheritdoc/>
    public virtual bool ContainsKey(string key) => Data.ContainsKey(key);

    /// <inheritdoc/>
    public virtual bool Remove(string key) => Data.Remove(key);

    /// <inheritdoc/>
    public virtual bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        => Data.TryGetValue(key, out value);

    /// <inheritdoc/>
    public virtual void Clear() => Data.Clear();

    /// <inheritdoc/>
    public virtual IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();

#pragma warning disable CA1033 // 接口方法应可由子类型调用
    bool ICollection<KeyValuePair<string, string>>.IsReadOnly => ((ICollection<KeyValuePair<string, string>>)Data).IsReadOnly;

    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)Data).Add(item);

    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)Data).CopyTo(array, arrayIndex);

    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)Data).Remove(item);

    bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) => Data.Contains(item);
#pragma warning restore CA1033 // 接口方法应可由子类型调用
}