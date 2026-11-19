namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Rules;

/// <summary>
/// 规则组：一个文件分类（如 <c>rule</c>、<c>art</c>）下的全部规则。
/// </summary>
public sealed class RuleGroup
{
    /// <summary>
    /// 节定义，键为定义名（实体类型名）。
    /// </summary>
    public Dictionary<string, SectionDefinitionRule> Definitions { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// 注册表规则，键为承载注册表的实体名。
    /// </summary>
    public Dictionary<string, RegistryRule> Registries { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// 全局规则，键为入口实体名。
    /// </summary>
    public Dictionary<string, GlobalRule> Globals { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// 枚举节规则，键为实体名。
    /// </summary>
    public Dictionary<string, EnumSectionRule> EnumSections { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// 发现规则集合。
    /// </summary>
    public List<DiscoverRule> Discover { get; } = [];

    /// <summary>
    /// 复合类型定义，键为类型名。
    /// </summary>
    public Dictionary<string, TypeDefinition> Types { get; } = [with(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// 枚举定义，键为枚举名。
    /// </summary>
    public Dictionary<string, EnumDefinition> Enums { get; } = [with(StringComparer.OrdinalIgnoreCase)];
}

/// <summary>
/// 枚举节规则：声明属性键或值的枚举约束。
/// </summary>
/// <param name="Section">实体名。</param>
/// <param name="Enum">枚举定义名；为 <see langword="null"/> 表示不作枚举约束。</param>
/// <param name="ValueType">值类型名；为 <see langword="null"/> 表示按枚举或字符串处理。</param>
/// <param name="List">值列表分隔符；为 <see langword="null"/> 表示单值。</param>
public sealed record class EnumSectionRule(string Section, string? Enum, string? ValueType, string? List);
