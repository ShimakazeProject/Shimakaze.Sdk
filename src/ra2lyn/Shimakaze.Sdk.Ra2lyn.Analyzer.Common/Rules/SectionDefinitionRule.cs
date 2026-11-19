namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Rules;

/// <summary>
/// 节定义规则：声明一类实体允许出现的键。
/// </summary>
/// <param name="name">定义名，等同实体类型名。</param>
public sealed class SectionDefinitionRule(string name)
{
    /// <summary>
    /// 定义名。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 父定义名，用于继承键规则。
    /// </summary>
    public string? Base { get; set; }

    /// <summary>
    /// 本定义直接声明的键。
    /// </summary>
    public Dictionary<string, KeyRule> Keys { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// 沿 <see cref="Base"/> 链合并得到的键集合（子定义覆盖父定义）。
    /// </summary>
    public Dictionary<string, KeyRule> GetEffectiveKeys(RuleGroup group)
    {
        Dictionary<string, KeyRule> result = [with(StringComparer.OrdinalIgnoreCase)];
        HashSet<string> visiting = [with(StringComparer.OrdinalIgnoreCase)];
        Collect(group, result, visiting);
        return result;
    }

    private void Collect(RuleGroup group, Dictionary<string, KeyRule> result, HashSet<string> visiting)
    {
        // 先收集父定义，再让本定义覆盖，保证子定义的键优先。
        if (Base is not null
            && visiting.Add(Name)
            && group.Definitions.TryGetValue(Base, out var parent))
        {
            parent.Collect(group, result, visiting);
        }

        foreach ((string key, KeyRule rule) in Keys)
            result[key] = rule;
    }
}
