namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Rules;

/// <summary>
/// 规则键的查找工具：区分“必须出现在当前组”的规则与“可继承自默认组”的类型/枚举。
/// </summary>
/// <remarks>
/// 顶层 <c>Type</c>/<c>Enum</c> 登记在 <see cref="RuleSetReader.DefaultGroupName"/> 组，供所有规则组共享；
/// 组内的 <c>SectionDefinition</c> 只在本组内查找。
/// </remarks>
public static class RuleSetLookup
{
    /// <summary>
    /// 查找复合类型定义：先本组，后默认组。
    /// </summary>
    public static bool TryGetType(RuleSet rules, string groupName, string typeName, out TypeDefinition definition)
    {
        if (TryGetGroupType(rules, groupName, typeName, out definition))
            return true;

        return TryGetGroupType(rules, RuleSetReader.DefaultGroupName, typeName, out definition);
    }

    /// <summary>
    /// 查找枚举定义：先本组，后默认组。
    /// </summary>
    public static bool TryGetEnum(RuleSet rules, string groupName, string enumName, out EnumDefinition definition)
    {
        if (TryGetGroupEnum(rules, groupName, enumName, out definition))
            return true;

        return TryGetGroupEnum(rules, RuleSetReader.DefaultGroupName, enumName, out definition);
    }

    /// <summary>
    /// 合并本组与默认组的复合类型定义（本组优先）。
    /// </summary>
    public static Dictionary<string, TypeDefinition> GetTypes(RuleSet rules, string groupName)
        => Merge(rules.FindGroup(groupName)?.Types, rules.FindGroup(RuleSetReader.DefaultGroupName)?.Types);

    /// <summary>
    /// 合并本组与默认组的枚举定义（本组优先）。
    /// </summary>
    public static Dictionary<string, EnumDefinition> GetEnums(RuleSet rules, string groupName)
        => Merge(rules.FindGroup(groupName)?.Enums, rules.FindGroup(RuleSetReader.DefaultGroupName)?.Enums);

    /// <summary>
    /// 查找节定义：只在本组内。
    /// </summary>
    public static SectionDefinitionRule? FindDefinition(RuleSet rules, string groupName, string definitionName)
        => rules.FindGroup(groupName) is { } group && group.Definitions.TryGetValue(definitionName, out var definition)
        ? definition
        : null;

    private static bool TryGetGroupType(RuleSet rules, string groupName, string typeName, out TypeDefinition definition)
    {
        RuleGroup? group = rules.FindGroup(groupName);
        if (group is not null && group.Types.TryGetValue(typeName, out var found))
        {
            definition = found;
            return true;
        }

        definition = null!;
        return false;
    }

    private static bool TryGetGroupEnum(RuleSet rules, string groupName, string enumName, out EnumDefinition definition)
    {
        RuleGroup? group = rules.FindGroup(groupName);
        if (group is not null && group.Enums.TryGetValue(enumName, out var found))
        {
            definition = found;
            return true;
        }

        definition = null!;
        return false;
    }

    private static Dictionary<string, TValue> Merge<TValue>(
        Dictionary<string, TValue>? local,
        Dictionary<string, TValue>? shared)
    {
        Dictionary<string, TValue> result = [with(StringComparer.OrdinalIgnoreCase)];
        foreach ((string key, TValue value) in shared ?? [])
            result[key] = value;

        foreach ((string key, TValue value) in local ?? [])
            result[key] = value;

        return result;
    }
}
