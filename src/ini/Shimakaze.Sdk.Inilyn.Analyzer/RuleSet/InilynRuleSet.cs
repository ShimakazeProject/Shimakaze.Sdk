using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// Inilyn 规则集：多个平台配置合并后的最终模型。
/// </summary>
public sealed class InilynRuleSet
{
    private readonly Dictionary<string, InilynValueType> _types = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InilynEnum> _enums = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InilynRuleGroup> _groups = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 值类型表（共享命名空间）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynValueType> Types => _types;

    /// <summary>
    /// 枚举表（共享命名空间）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynEnum> Enums => _enums;

    /// <summary>
    /// 规则组表。
    /// </summary>
    public IReadOnlyDictionary<string, InilynRuleGroup> Groups => _groups;

    /// <summary>
    /// 加载多个平台配置并合并（加载顺序决定合并优先级，后加载者覆盖/扩展先加载者）。
    /// </summary>
    /// <param name="platformConfigPaths">平台配置文件路径数组（如 vanilla.xml、ares.xml）。</param>
    /// <returns>合并后的规则集。</returns>
    public static InilynRuleSet Load(IEnumerable<string> platformConfigPaths)
    {
        return InilynRuleSetLoader.Load(platformConfigPaths);
    }

    /// <summary>
    /// 获取值类型。
    /// </summary>
    /// <param name="name">类型名。</param>
    /// <returns>值类型，不存在时返回 <see langword="null"/>。</returns>
    public InilynValueType? GetType(string name)
    {
        return _types.GetValueOrDefault(name);
    }

    /// <summary>
    /// 获取枚举。
    /// </summary>
    /// <param name="name">枚举名。</param>
    /// <returns>枚举，不存在时返回 <see langword="null"/>。</returns>
    public InilynEnum? GetEnum(string name)
    {
        return _enums.GetValueOrDefault(name);
    }

    /// <summary>
    /// 获取规则组。
    /// </summary>
    /// <param name="name">组名。</param>
    /// <param name="group">输出的组。</param>
    /// <returns>是否找到。</returns>
    public bool TryGetGroup(string name, [NotNullWhen(true)] out InilynRuleGroup? group)
    {
        return _groups.TryGetValue(name, out group);
    }

    internal void AddType(InilynValueType type)
    {
        _types[type.Name] = type;
    }

    internal void AddEnum(InilynEnum en)
    {
        if (_enums.TryGetValue(en.Name, out var existing))
        {
            existing.AddRange(en.Values);
        }
        else
        {
            _enums[en.Name] = en;
        }
    }

    internal InilynRuleGroup GetOrAddGroup(string name)
    {
        if (!_groups.TryGetValue(name, out var group))
        {
            group = new InilynRuleGroup(name);
            _groups[name] = group;
        }

        return group;
    }
}
