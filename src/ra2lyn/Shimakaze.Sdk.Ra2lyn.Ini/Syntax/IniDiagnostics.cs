using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 前端诊断工厂：按既有 <c>INIxxx</c> 诊断码构造诊断，文案统一为中文。
/// </summary>
public static class IniDiagnostics
{
    /// <summary>
    /// INI001：缺少左方括号 <c>[</c>。
    /// </summary>
    public static Diagnostic MissingLeftBracket(SourceSpan span)
        => Make(DiagnosticCodes.MissingLeftBracket, DiagnosticSeverity.Error, "缺少 '['", span);

    /// <summary>
    /// INI002：缺少右方括号 <c>]</c>。
    /// </summary>
    public static Diagnostic MissingRightBracket(SourceSpan span)
        => Make(DiagnosticCodes.MissingRightBracket, DiagnosticSeverity.Error, "缺少 ']'", span);

    /// <summary>
    /// INI003：缺少节名。
    /// </summary>
    public static Diagnostic MissingName(SourceSpan span)
        => Make(DiagnosticCodes.MissingName, DiagnosticSeverity.Error, "缺少节名", span);

    /// <summary>
    /// INI004：缺少继承冒号 <c>:</c>。
    /// </summary>
    public static Diagnostic MissingColon(SourceSpan span)
        => Make(DiagnosticCodes.MissingColon, DiagnosticSeverity.Error, "缺少 ':'", span);

    /// <summary>
    /// INI005：缺少继承逗号 <c>,</c>。
    /// </summary>
    public static Diagnostic MissingComma(SourceSpan span)
        => Make(DiagnosticCodes.MissingComma, DiagnosticSeverity.Error, "缺少 ','", span);

    /// <summary>
    /// INI006：缺少等号 <c>=</c>。
    /// </summary>
    /// <param name="key">出问题的键名。</param>
    /// <param name="span">诊断位置。</param>
    public static Diagnostic MissingEquals(string key, SourceSpan span)
        => Make(DiagnosticCodes.MissingEquals, DiagnosticSeverity.Error, $"键 '{key}' 缺少 '='", span);

    /// <summary>
    /// INI007：缺少键名。
    /// </summary>
    public static Diagnostic MissingKey(SourceSpan span)
        => Make(DiagnosticCodes.MissingKey, DiagnosticSeverity.Error, "缺少键名", span);

    /// <summary>
    /// INI008：缺少值，以警告级别报告。
    /// </summary>
    public static Diagnostic MissingValue(SourceSpan span)
        => Make(DiagnosticCodes.MissingValue, DiagnosticSeverity.Warning, "缺少值", span);

    /// <summary>
    /// INI009：键名中包含空白。
    /// </summary>
    /// <param name="key">含空白的键名。</param>
    /// <param name="span">诊断位置。</param>
    public static Diagnostic WhitespaceInKey(string key, SourceSpan span)
        => Make(DiagnosticCodes.WhitespaceInKey, DiagnosticSeverity.Error, $"键名 '{key}' 中包含空白", span);

    /// <summary>
    /// INI010：无法识别的记号，符号被跳过。
    /// </summary>
    /// <param name="text">无法识别的记号文本。</param>
    /// <param name="span">诊断位置。</param>
    public static Diagnostic UnexpectedToken(string text, SourceSpan span)
        => Make(DiagnosticCodes.UnexpectedToken, DiagnosticSeverity.Error, $"无法识别的记号: '{text}'", span);

    /// <summary>
    /// INI099：兜底语法错误。
    /// </summary>
    /// <param name="message">错误描述。</param>
    /// <param name="span">诊断位置。</param>
    public static Diagnostic SyntaxError(string message, SourceSpan span)
        => Make(DiagnosticCodes.SyntaxError, DiagnosticSeverity.Error, message, span);

    /// <summary>
    /// INI015：模板节头 <c>[$Name] :</c> 之后既不是 <c>[</c> 继承列表，也不是 <c>where</c> 关键字。
    /// </summary>
    public static Diagnostic TemplateWhereExpected(SourceSpan span)
        => Make(DiagnosticCodes.TemplateWhereExpected, DiagnosticSeverity.Error, "模板节 ':' 之后应为 '[' 继承列表或 'where' 表达式", span);

    /// <summary>
    /// INI016：模板节头 <c>[$Name] : where</c> 之后缺少表达式。
    /// </summary>
    public static Diagnostic TemplateWhereMissing(SourceSpan span)
        => Make(DiagnosticCodes.TemplateWhereMissing, DiagnosticSeverity.Error, "'where' 之后缺少表达式", span);

    private static Diagnostic Make(string code, DiagnosticSeverity severity, string message, SourceSpan span)
        => new(code, severity, message, span);
}
