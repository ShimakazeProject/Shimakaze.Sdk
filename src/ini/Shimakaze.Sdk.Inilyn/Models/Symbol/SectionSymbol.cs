
using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Inilyn.Models.Symbol;

/// <summary>
/// 表示配置文件中的一个节（Section），包含一组键值对，并支持继承其他节。
/// </summary>
public sealed class SectionSymbol : Symbol
{
    internal readonly Dictionary<string, KeySymbol> Keys = [];

    internal SectionSymbol(string name, string? description, string? inherit)
    {
        SectionName = name;
        Description = description;
        Inherit = inherit;
    }

    /// <summary>
    /// 获取当前节的名称。
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// 获取当前节的描述信息。
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// 获取当前节所继承的父节（如果存在）。
    /// </summary>
    public string? Inherit { get; }

    /// <summary>
    /// 获取当前节中定义的所有键。
    /// </summary>
    public IEnumerable<KeySymbol> Data => Keys.Values;

    /// <inheritdoc/>
    public override string Name => SectionName;


    /// <summary>
    /// 根据键名获取当前节中定义的键。
    /// </summary>
    /// <param name="name">要查找的键名。</param>
    /// <returns>匹配的键对象。</returns>
    /// <exception cref="KeyNotFoundException">如果未找到指定名称的键。</exception>
    public KeySymbol GetKey(string name) => Keys[name];

    /// <summary>
    /// 尝试根据键名获取当前节中定义的键。
    /// </summary>
    /// <param name="name">要查找的键名。</param>
    /// <param name="key">如果找到则返回对应的键对象；否则返回 null。</param>
    /// <returns>如果找到指定名称的键，则为 true；否则为 false。</returns>
    public bool TryGetKey(string name, [NotNullWhen(true)] out KeySymbol? key) => Keys.TryGetValue(name, out key);

    /// <summary>
    /// 判断当前节是否包含指定名称的键。
    /// </summary>
    /// <param name="name">要检查的键名。</param>
    /// <returns>如果当前节包含指定名称的键，则为 true；否则为 false。</returns>
    public bool ContainsKey(string name) => Keys.ContainsKey(name);
}
