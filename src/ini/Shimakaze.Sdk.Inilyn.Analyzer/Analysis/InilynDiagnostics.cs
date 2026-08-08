using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Analyzer.Analysis;

/// <summary>
/// 集中管理所有分析诊断描述符（懒加载的运行时常量）。
/// 编号规则：分析器诊断从 INI500 开始分配。
/// </summary>
public static class InilynDiagnostics
{
    /// <summary>INI500: 未知的节（未被任何规则识别）。</summary>
    public static readonly DiagnosticDescriptor UnknownSection = new(
        "INI500", "未知的节", "未知的节 [{0}]（未被任何规则识别）", "Analysis", DiagnosticSeverity.Warning);

    /// <summary>INI501: 注册表成员没有对应的节。</summary>
    public static readonly DiagnosticDescriptor RegistryMemberMissingSection = new(
        "INI501", "注册表成员缺少节", "注册表 [{0}] 的成员 '{1}' 没有对应的节（应在 {2} 组）", "Analysis", DiagnosticSeverity.Warning);

    /// <summary>INI502: 未知的键。</summary>
    public static readonly DiagnosticDescriptor UnknownKey = new(
        "INI502", "未知的键", "节 [{0}] 存在未知的键 '{1}'", "Analysis", DiagnosticSeverity.Warning);

    /// <summary>INI503: 值类型不匹配。</summary>
    public static readonly DiagnosticDescriptor ValueTypeMismatch = new(
        "INI503", "值类型不匹配", "节 [{0}] 的键 '{1}' 的值 '{2}' 不符合类型 {3}", "Analysis", DiagnosticSeverity.Error);

    /// <summary>INI504: 枚举值非法（值不匹配）。</summary>
    public static readonly DiagnosticDescriptor EnumValueInvalid = new(
        "INI504", "枚举值非法", "节 [{0}] 的键 '{1}' 的值 '{2}' 不符合类型 {3}", "Analysis", DiagnosticSeverity.Error);

    /// <summary>INI505: 枚举节的键不是合法成员。</summary>
    public static readonly DiagnosticDescriptor EnumKeyInvalid = new(
        "INI505", "枚举值非法", "枚举节 [{0}] 的键 '{1}' 不是枚举 {2} 的合法成员", "Analysis", DiagnosticSeverity.Error);

    /// <summary>INI506: 引用目标不存在。</summary>
    public static readonly DiagnosticDescriptor ReferenceMissing = new(
        "INI506", "引用目标不存在", "节 [{0}] 的键 '{1}' 的值 '{2}' 不符合类型 {3}", "Analysis", DiagnosticSeverity.Error);

    /// <summary>INI507: 发现规则引用的目标节不存在。</summary>
    public static readonly DiagnosticDescriptor DiscoverTargetMissing = new(
        "INI507", "发现目标不存在", "发现规则：{0} 组的节 [{1}] 引用的目标节 [{2}] 不存在于 {3} 组", "Analysis", DiagnosticSeverity.Error);

    /// <summary>INI508: 节被归为多个类型。</summary>
    public static readonly DiagnosticDescriptor MultiType = new(
        "INI508", "多类型归属", "节 [{0}] 同时被归为多个类型：{1}", "Analysis", DiagnosticSeverity.Warning);

    /// <summary>INI509: 节不可达。</summary>
    public static readonly DiagnosticDescriptor Unreachable = new(
        "INI509", "节不可达", "节 [{0}] 不可达，可被 TreeShaking 移除", "Analysis", DiagnosticSeverity.Info);
}
