
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Inilyn.Models.Symbol;

/// <summary>
/// 表示配置文件中的一个节（Section），包含一组键值对，并支持继承其他节。
/// </summary>
public sealed class SectionSymbol(string name, string description, SectionSymbol? inherit, params IEnumerable<KeySymbol> data) : Symbol
{
    private readonly IReadOnlyDictionary<string, KeySymbol> _keys = Parse(data);

    /// <summary>
    /// 获取当前节的名称。
    /// </summary>
    public string SectionName { get; } = name;

    /// <summary>
    /// 获取当前节的描述信息。
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// 获取当前节所继承的父节（如果存在）。
    /// </summary>
    public SectionSymbol? Inherit { get; } = inherit;

    /// <summary>
    /// 获取当前节中定义的所有键。
    /// </summary>
    public IEnumerable<KeySymbol> Data => _keys.Values;

    /// <inheritdoc/>
    public override string Name => SectionName;

    /// <summary>
    /// 将键集合解析为字典形式，用于快速查找。
    /// </summary>
    /// <param name="data">要解析的键集合。</param>
    /// <returns>表示键名到键对象映射的只读字典。</returns>
    private static IReadOnlyDictionary<string, KeySymbol> Parse(IEnumerable<KeySymbol> data)
    {
        Dictionary<string, KeySymbol> keys = [];
        foreach (var key in data)
        {
            if (keys.ContainsKey(key.Name))
            {
                // TODO: Diagnostic
            }

            keys[key.Name] = key;
        }

        return keys.AsReadOnly();
    }

    /// <summary>
    /// 根据键名获取当前节中定义的键。
    /// </summary>
    /// <param name="name">要查找的键名。</param>
    /// <returns>匹配的键对象。</returns>
    /// <exception cref="KeyNotFoundException">如果未找到指定名称的键。</exception>
    public KeySymbol GetKey(string name) => _keys[name];

    /// <summary>
    /// 尝试根据键名获取当前节中定义的键。
    /// </summary>
    /// <param name="name">要查找的键名。</param>
    /// <param name="key">如果找到则返回对应的键对象；否则返回 null。</param>
    /// <returns>如果找到指定名称的键，则为 true；否则为 false。</returns>
    public bool TryGetKey(string name, [NotNullWhen(true)] out KeySymbol? key) => _keys.TryGetValue(name, out key);

    /// <summary>
    /// 判断当前节是否包含指定名称的键。
    /// </summary>
    /// <param name="name">要检查的键名。</param>
    /// <returns>如果当前节包含指定名称的键，则为 true；否则为 false。</returns>
    public bool ContainsKey(string name) => _keys.ContainsKey(name);
}
