
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Inilyn.Models.Emit;

/// <summary>
/// INI节
/// </summary>
/// <param name="name"></param>
/// <param name="data"></param>
#pragma warning disable CA1710 // 标识符应具有正确的后缀
public sealed class IniSection(string name, IEnumerable<KeyValuePair<string, string>> data) : IReadOnlyDictionary<string, string>
#pragma warning restore CA1710 // 标识符应具有正确的后缀
{
    private readonly Dictionary<string, string> _data = new(data);

    /// <summary>
    /// 获取键值对值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string this[string key] => _data[key];

    /// <summary>
    /// 获取节名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 获取所有键
    /// </summary>
    public IEnumerable<string> Keys => _data.Keys;

    /// <summary>
    /// 获取所有值
    /// </summary>
    public IEnumerable<string> Values => _data.Values;

    /// <summary>
    /// 获取键值对数量
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// 判断键是否存在
    /// </summary>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _data.GetEnumerator();

    /// <summary>
    /// 尝试获取键值对值
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _data.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
