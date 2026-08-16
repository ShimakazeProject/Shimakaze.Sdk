namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// 集中管理所有内部诊断描述符（懒加载的运行时常量）。
/// </summary>
public static class Diagnostics
{
    // ── 语法分析 ──

    /// <summary>INI001: 期望节名。</summary>
    public static readonly DiagnosticDescriptor ExpectedSectionName = new(
        "INI001", "期望节名", "期望节名", "Syntax", DiagnosticSeverity.Error);

    /// <summary>INI002: 期望 ']'。</summary>
    public static readonly DiagnosticDescriptor ExpectedRightBracket = new(
        "INI002", "期望 ']'", "期望 ']'", "Syntax", DiagnosticSeverity.Error);

    /// <summary>INI003: 期望 Mixin 引用的节名。</summary>
    public static readonly DiagnosticDescriptor ExpectedMixinSectionName = new(
        "INI003", "期望 Mixin 引用的节名", "期望 Mixin 引用的节名", "Syntax", DiagnosticSeverity.Error);

    /// <summary>INI004: Mixin 引用中期望 ']'。</summary>
    public static readonly DiagnosticDescriptor ExpectedMixinRightBracket = new(
        "INI004", "期望 ']'", "期望 ']'", "Syntax", DiagnosticSeverity.Error);

    /// <summary>INI005: 期望 '='。</summary>
    public static readonly DiagnosticDescriptor ExpectedEqualSign = new(
        "INI005", "期望 '='", "期望 '='", "Syntax", DiagnosticSeverity.Error);

    /// <summary>INI006: 期望值。</summary>
    public static readonly DiagnosticDescriptor ExpectedValue = new(
        "INI006", "期望值", "期望值", "Syntax", DiagnosticSeverity.Error);

    /// <summary>INI099: 意外的记号。</summary>
    public static readonly DiagnosticDescriptor UnexpectedToken = new(
        "INI099", "意外的记号", "意外的记号 '{0}'", "Syntax", DiagnosticSeverity.Error);

    // ── 语义分析 ──

    /// <summary>INI101: 重复的节声明。</summary>
    public static readonly DiagnosticDescriptor DuplicateSection = new(
        "INI101", "重复的节声明", "重复的节声明 '{0}'", "Semantic", DiagnosticSeverity.Warning);

    /// <summary>INI102: 重复的键（同节内）。</summary>
    public static readonly DiagnosticDescriptor DuplicateKey = new(
        "INI102", "重复的键", "重复的键 '{0}'", "Semantic", DiagnosticSeverity.Warning);

    /// <summary>INI103: Mixin 引用的节不存在。</summary>
    public static readonly DiagnosticDescriptor MixinSectionNotFound = new(
        "INI103", "Mixin 引用的节不存在", "Mixin 引用的节 '{0}' 不存在", "Semantic", DiagnosticSeverity.Error);

    /// <summary>INI104: Mixin 循环引用。</summary>
    public static readonly DiagnosticDescriptor MixinCircularReference = new(
        "INI104", "Mixin 循环引用", "检测到 Mixin 循环引用: {0}", "Semantic", DiagnosticSeverity.Error);

    // ── TreeShaking ──

    /// <summary>INI201: 节被移除（未被引用）。</summary>
    public static readonly DiagnosticDescriptor SectionRemoved = new(
        "INI201", "节被移除", "节 '{0}' 未被任何入口节引用，已移除", "TreeShaking", DiagnosticSeverity.Warning);
}
