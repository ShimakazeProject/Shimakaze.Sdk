namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 解析后的值类型引用。
/// </summary>
public enum InilynValueRefKind
{
    /// <summary>
    /// 内置类型（int/float/boolean/string/percent）。
    /// </summary>
    Builtin,

    /// <summary>
    /// 枚举。
    /// </summary>
    Enum,

    /// <summary>
    /// 元组类型。
    /// </summary>
    Tuple,

    /// <summary>
    /// 外部资源。
    /// </summary>
    External,

    /// <summary>
    /// 引用某组的节（同组或跨组，值为节名）。
    /// </summary>
    SectionRef,

    /// <summary>
    /// 引用某组注册表里的成员（值为注册表列出的名字）。
    /// </summary>
    RegistryRef,
}

/// <summary>
/// 解析后的值类型。
/// </summary>
/// <param name="Kind">类型种类。</param>
/// <param name="BuiltinName">内置类型名。</param>
/// <param name="EnumName">枚举名。</param>
/// <param name="TupleName">元组类型名。</param>
/// <param name="ExternalName">外部资源种类。</param>
/// <param name="TargetGroup">引用目标组（<see langword="null"/> 表示同组）。</param>
/// <param name="TargetName">引用目标名（节定义名或注册表节名）。</param>
public sealed record class InilynResolvedValueType(
    InilynValueRefKind Kind,
    string? BuiltinName = null,
    string? EnumName = null,
    string? TupleName = null,
    string? ExternalName = null,
    string? TargetGroup = null,
    string? TargetName = null)
{
    /// <summary>
    /// 是否为引用（节引用或注册表成员引用）。
    /// </summary>
    public bool IsReference => Kind is InilynValueRefKind.SectionRef or InilynValueRefKind.RegistryRef;

    /// <summary>
    /// 创建内置类型。
    /// </summary>
    public static InilynResolvedValueType OfBuiltin(string name) => new(InilynValueRefKind.Builtin, BuiltinName: name);

    /// <summary>
    /// 创建枚举。
    /// </summary>
    public static InilynResolvedValueType OfEnum(string name) => new(InilynValueRefKind.Enum, EnumName: name);

    /// <summary>
    /// 创建元组。
    /// </summary>
    public static InilynResolvedValueType OfTuple(string name) => new(InilynValueRefKind.Tuple, TupleName: name);

    /// <summary>
    /// 创建外部资源。
    /// </summary>
    public static InilynResolvedValueType OfExternal(string kind) => new(InilynValueRefKind.External, ExternalName: kind);

    /// <summary>
    /// 创建节引用。
    /// </summary>
    public static InilynResolvedValueType OfSectionRef(string? group, string name) => new(InilynValueRefKind.SectionRef, TargetGroup: group, TargetName: name);

    /// <summary>
    /// 创建注册表成员引用。
    /// </summary>
    public static InilynResolvedValueType OfRegistryRef(string? group, string name) => new(InilynValueRefKind.RegistryRef, TargetGroup: group, TargetName: name);
}

/// <summary>
/// 值类型引用解析器。
/// </summary>
public static class InilynValueRefResolver
{
    /// <summary>
    /// 解析全部成员（支持用 <c>|</c> 分隔的联合类型，如 <c>BuildingType|PrerequisiteKeyword</c>）。
    /// </summary>
    /// <param name="ruleSet">规则集。</param>
    /// <param name="groupName">键所在组。</param>
    /// <param name="raw">类型文本。</param>
    /// <returns>解析后的值类型列表。</returns>
    public static IReadOnlyList<InilynResolvedValueType> ResolveAll(InilynRuleSet ruleSet, string groupName, string raw)
    {
        if (!raw.Contains('|'))
        {
            return [Resolve(ruleSet, groupName, raw)];
        }

        return [.. raw
            .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(r => Resolve(ruleSet, groupName, r))];
    }

    /// <summary>
    /// 将键的类型文本解析为具体的值类型引用。
    /// </summary>
    /// <param name="ruleSet">规则集。</param>
    /// <param name="groupName">键所在组。</param>
    /// <param name="raw">类型文本（如 <c>int</c>、<c>Weapon</c>、<c>Art.ObjectType</c>）。</param>
    /// <returns>解析后的值类型。</returns>
    public static InilynResolvedValueType Resolve(InilynRuleSet ruleSet, string groupName, string raw)
    {
        int dot = raw.IndexOf('.');
        if (dot > 0 && dot < raw.Length - 1)
        {
            string group = raw[..dot];
            string name = raw[(dot + 1)..];
            if (ruleSet.TryGetGroup(group, out var targetGroup))
            {
                if (targetGroup.Registries.ContainsKey(name))
                {
                    return InilynResolvedValueType.OfRegistryRef(group, name);
                }

                return InilynResolvedValueType.OfSectionRef(group, name);
            }

            // 目标组不存在：仍按节引用处理，验证时会报错
            return InilynResolvedValueType.OfSectionRef(group, name);
        }

        if (InilynValueType.BuiltinNames.Contains(raw))
        {
            return InilynResolvedValueType.OfBuiltin(raw);
        }

        if (ruleSet.GetEnum(raw) is not null)
        {
            return InilynResolvedValueType.OfEnum(raw);
        }

        if (ruleSet.GetType(raw) is { } customType)
        {
            return customType.Kind switch
            {
                InilynValueTypeKind.Tuple => InilynResolvedValueType.OfTuple(raw),
                InilynValueTypeKind.External => InilynResolvedValueType.OfExternal(customType.ExternalKind ?? raw),
                _ => InilynResolvedValueType.OfBuiltin(raw),
            };
        }

        // 同组引用
        if (ruleSet.TryGetGroup(groupName, out var g))
        {
            if (g.Registries.ContainsKey(raw))
            {
                return InilynResolvedValueType.OfRegistryRef(null, raw);
            }
        }

        return InilynResolvedValueType.OfSectionRef(null, raw);
    }
}
