namespace Shimakaze.Sdk.Analyzer.Common.Rules;

/// <summary>
/// 规则集：按规则组组织的一组规则。
/// </summary>
public sealed class RuleSet
{
    /// <summary>
    /// 规则组，键为组名。
    /// </summary>
    public Dictionary<string, RuleGroup> Groups { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 找不到指定规则组时返回 <see langword="null"/>。
    /// </summary>
    public RuleGroup? FindGroup(string group)
        => Groups.TryGetValue(group, out var result) ? result : null;
}
