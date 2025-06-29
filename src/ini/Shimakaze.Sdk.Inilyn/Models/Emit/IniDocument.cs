
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Inilyn.Models.Emit;

/// <summary>
/// INI文档
/// </summary>
#pragma warning disable CA1710 // 标识符应具有正确的后缀
public sealed class IniDocument(IEnumerable<KeyValuePair<string, IniSection>> data) : IReadOnlyDictionary<string, IniSection>
#pragma warning restore CA1710 // 标识符应具有正确的后缀
{
    private readonly Dictionary<string, IniSection> _data = new(data);

    /// <summary>
    /// 获取指定名称的节
    /// </summary>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public IniSection this[string sectionName] => _data[sectionName];

    /// <summary>
    /// 获取所有节名称
    /// </summary>
    public IEnumerable<string> SectionNames => _data.Keys;

    /// <summary>
    /// 获取所有节
    /// </summary>
    public IEnumerable<IniSection> Sections => _data.Values;

    /// <summary>
    /// 获取节数量
    /// </summary>
    public int Count => _data.Count;

    IEnumerable<string> IReadOnlyDictionary<string, IniSection>.Keys => SectionNames;

    IEnumerable<IniSection> IReadOnlyDictionary<string, IniSection>.Values => Sections;

    /// <summary>
    /// 获取是否存在指定节
    /// </summary>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public bool ContainsSection(string sectionName) => _data.ContainsKey(sectionName);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, IniSection>> GetEnumerator() => _data.GetEnumerator();

    /// <summary>
    /// 尝试获取节
    /// </summary>
    /// <param name="sectionName">节名称</param>
    /// <param name="section">节</param>
    /// <returns></returns>
    public bool TryGetSection(string sectionName, [MaybeNullWhen(false)] out IniSection section) => _data.TryGetValue(sectionName, out section);

    bool IReadOnlyDictionary<string, IniSection>.ContainsKey(string key) => ContainsSection(key);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool IReadOnlyDictionary<string, IniSection>.TryGetValue(string key, [MaybeNullWhen(false)] out IniSection value) => TryGetSection(key, out value);
}
