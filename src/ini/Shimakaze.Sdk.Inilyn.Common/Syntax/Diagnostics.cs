using LsDiagnosticSeverity = Shimakaze.LanguageServerProtocol.Model.DiagnosticSeverity;

namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// 预定义诊断描述符。
/// </summary>
public static class Diagnostics
{
    // ── Syntax ────────────────────────────────────────────────────────

    /// <summary>
    /// INI001：缺少 <c>[</c>。
    /// </summary>
    public static readonly DiagnosticDescriptor INI001 = new(
        "INI001", "缺少 '['",
        "缺少 '['",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI002：缺少 <c>]</c>。
    /// </summary>
    public static readonly DiagnosticDescriptor INI002 = new(
        "INI002", "缺少 ']'",
        "缺少 ']'",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI003：缺少节名称。
    /// </summary>
    public static readonly DiagnosticDescriptor INI003 = new(
        "INI003", "缺少节名称",
        "缺少节名称",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI004：缺少 <c>:</c>。
    /// </summary>
    public static readonly DiagnosticDescriptor INI004 = new(
        "INI004", "缺少 ':'",
        "缺少 ':'",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI005：缺少 <c>,</c>。
    /// </summary>
    public static readonly DiagnosticDescriptor INI005 = new(
        "INI005", "缺少 ','",
        "缺少 ','",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI006：缺少 <c>=</c>。
    /// </summary>
    public static readonly DiagnosticDescriptor INI006 = new(
        "INI006", "缺少 '='",
        "缺少 '='",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI007：缺少键。
    /// </summary>
    public static readonly DiagnosticDescriptor INI007 = new(
        "INI007", "缺少键",
        "缺少键",
        "Syntax", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI008：缺少值。
    /// </summary>
    public static readonly DiagnosticDescriptor INI008 = new(
        "INI008", "缺少值",
        "缺少值",
        "Syntax", LsDiagnosticSeverity.Warning);

    /// <summary>
    /// INI099：意外的标记。
    /// </summary>
    public static readonly DiagnosticDescriptor INI099 = new(
        "INI099", "意外的标记",
        "意外的标记: '{0}'",
        "Syntax", LsDiagnosticSeverity.Error);

    // ── Semantic ──────────────────────────────────────────────────────

    /// <summary>
    /// INI101：重复的节名称。
    /// </summary>
    public static readonly DiagnosticDescriptor INI101 = new(
        "INI101", "重复的节名称",
        "重复的节名称: '{0}'",
        "Semantic", LsDiagnosticSeverity.Warning);

    /// <summary>
    /// INI102：在节外部出现的键值对。
    /// </summary>
    public static readonly DiagnosticDescriptor INI102 = new(
        "INI102", "缺少所属节",
        "键值对 '{0}' 不属于任何节",
        "Semantic", LsDiagnosticSeverity.Error);

    /// <summary>
    /// INI103：在节内重复的键。
    /// </summary>
    public static readonly DiagnosticDescriptor INI103 = new(
        "INI103", "重复的键",
        "节 '{0}' 中存在重复的键 '{1}'",
        "Semantic", LsDiagnosticSeverity.Warning);

    // ── Rule Validation ──────────────────────────────────────────────

    /// <summary>
    /// INI201：键值类型不匹配。
    /// </summary>
    public static readonly DiagnosticDescriptor INI201 = new(
        "INI201", "类型不匹配",
        "节 '{0}' 的键 '{1}' 类型不匹配: {2}",
        "RuleValidation", LsDiagnosticSeverity.Warning);
}
