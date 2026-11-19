namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Rules;

/// <summary>
/// 注册表规则：声明某个节登记了哪些类型的成员。
/// </summary>
/// <param name="Element">登记的元素引用，形如 <c>Group.Type</c> 或 <c>Type</c>。</param>
public sealed record class RegistryRule(string Element);

/// <summary>
/// 全局规则：声明由系统固定引用的入口实体。
/// </summary>
/// <param name="Section">实体名。</param>
/// <param name="Type">实体类型名；为 <see langword="null"/> 时使用 <paramref name="Section"/>。</param>
public sealed record class GlobalRule(string Section, string? Type);

/// <summary>
/// 发现规则：从某类实体的某个键出发，发现目标类型的实体。
/// </summary>
/// <param name="From">来源类型名；为 <see langword="null"/> 表示不限制。</param>
/// <param name="ResolveKey">
/// 来源实体中承载目标名称的键；含 <c>#</c> 时按 <paramref name="Min"/>/<paramref name="Max"/> 展开。
/// </param>
/// <param name="Target">目标类型引用，形如 <c>Group.Name</c> 或 <c>Name</c>。</param>
/// <param name="Fallback">目标缺失时使用的兜底目标；为 <see langword="null"/> 表示不兜底。</param>
/// <param name="Min"><c>#</c> 展开的起始下标，可为数字或键名；为 <see langword="null"/> 表示 0。</param>
/// <param name="Max"><c>#</c> 展开的结束下标，可为数字或键名；为 <see langword="null"/> 表示不展开。</param>
public sealed record class DiscoverRule(
    string? From,
    string? ResolveKey,
    string Target,
    string? Fallback,
    string? Min,
    string? Max);
