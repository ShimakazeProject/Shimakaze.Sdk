using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI文档
/// </summary>
#pragma warning disable CA1710 // 标识符应具有正确的后缀
public abstract class IniDocument<TIniSection> : IIniDocument<TIniSection>, ICollection<TIniSection>, IDictionary<string, TIniSection>
#pragma warning restore CA1710 // 标识符应具有正确的后缀
    where TIniSection : IIniSection
{
    /// <summary>
    /// 在文档头部的没有包含在Section中的孤立的键值对
    /// </summary>
    public abstract TIniSection DefaultSection { get; }

    /// <summary>
    /// 内部的用于快速查询的Section字典
    /// </summary>
    private readonly Dictionary<string, TIniSection> _data = [];

    /// <inheritdoc/>
    public int Count => _data.Count;


    /// <inheritdoc/>
    [Obsolete("Use SectionNames instead of.")]
    public ICollection<string> Keys => _data.Keys;

    /// <inheritdoc/>
    [Obsolete("Use Sections instead of.")]
    public ICollection<TIniSection> Values => _data.Values;

    /// <inheritdoc/>
    public ICollection<string> SectionNames => _data.Keys;

    /// <inheritdoc/>
    public ICollection<TIniSection> Sections => _data.Values;

    /// <inheritdoc/>
    public TIniSection this[string section]
    {
        get => _data[section];
        set => _data[section] = value;
    }

    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="section">节名</param>
    /// <param name="key">键</param>
    /// <returns></returns>
    public string this[string section, string key]
    {
        get => _data[section][key];
        set => _data[section][key] = value;
    }

    /// <summary>
    /// 构造一个INI文档
    /// </summary>
    /// <param name="sections"> 节列表 </param>
    /// <param name="sectionNameComparer"></param>
    protected IniDocument(IEnumerable<TIniSection> sections, IEqualityComparer<string>? sectionNameComparer = default)
        : this(sectionNameComparer)
        => _data = sections.ToDictionary(i => i.Name, sectionNameComparer);

    /// <inheritdoc cref="IniDocument{TIniSection}.IniDocument(IEnumerable{TIniSection}, IEqualityComparer{string})"/>
    protected IniDocument(IEqualityComparer<string>? sectionNameComparer = default)
        => _data = new(sectionNameComparer);

    /// <inheritdoc/>
    public void Add(TIniSection item) => _data.Add(item.Name, item);
    void IDictionary<string, TIniSection>.Add(string key, TIniSection value) => Add(value);
    void ICollection<KeyValuePair<string, TIniSection>>.Add(KeyValuePair<string, TIniSection> item) => Add(item.Value);

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

    /// <inheritdoc/>
    public bool Remove(string key) => _data.Remove(key);
    bool ICollection<TIniSection>.Remove(TIniSection item) => Remove(item.Name);
    bool ICollection<KeyValuePair<string, TIniSection>>.Remove(KeyValuePair<string, TIniSection> item) => Remove(item.Key);

    /// <summary>
    /// TryGetSection
    /// </summary>
    /// <param name="key"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    public bool TryGetSection(string key, [MaybeNullWhen(false)] out TIniSection section) => _data.TryGetValue(key, out section);
    /// <inheritdoc/>
    [Obsolete("Use TryGetSection instead of.")]
    public virtual bool TryGetValue(string key, [MaybeNullWhen(false)] out TIniSection value)
        => _data.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IEnumerator<TIniSection> GetEnumerator() => _data.Values.GetEnumerator();

    IEnumerator<KeyValuePair<string, TIniSection>> IEnumerable<KeyValuePair<string, TIniSection>>.GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#pragma warning disable CA1033 // 接口方法应可由子类型调用
    bool ICollection<KeyValuePair<string, TIniSection>>.IsReadOnly => ((ICollection<KeyValuePair<string, TIniSection>>)_data).IsReadOnly;
    bool ICollection<TIniSection>.IsReadOnly => ((ICollection<KeyValuePair<string, TIniSection>>)_data).IsReadOnly;
    bool ICollection<TIniSection>.Contains(TIniSection item) => ContainsSection(item.Name);
    bool ICollection<KeyValuePair<string, TIniSection>>.Contains(KeyValuePair<string, TIniSection> item) => ContainsSection(item.Key);
    void ICollection<TIniSection>.CopyTo(TIniSection[] array, int arrayIndex) => Sections.CopyTo(array, arrayIndex);
    void ICollection<KeyValuePair<string, TIniSection>>.CopyTo(KeyValuePair<string, TIniSection>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, TIniSection>>)_data).CopyTo(array, arrayIndex);
#pragma warning restore CA1033 // 接口方法应可由子类型调用
}