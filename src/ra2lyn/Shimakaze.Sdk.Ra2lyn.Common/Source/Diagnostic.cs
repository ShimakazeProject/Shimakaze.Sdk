namespace Shimakaze.Sdk.Ra2lyn.Common.Source;

/// <summary>
/// 一条诊断信息。
/// </summary>
/// <param name="Code">诊断代码，沿用 <c>INIxxx</c> 前缀。</param>
/// <param name="Severity">严重级别。</param>
/// <param name="Message">面向用户的诊断消息。</param>
/// <param name="Span">诊断位置；<c>default</c> 表示没有位置。</param>
public sealed record class Diagnostic(
    string Code,
    DiagnosticSeverity Severity,
    string Message,
    SourceSpan Span)
{
    /// <summary>
    /// 诊断位置是否缺失。
    /// </summary>
    public bool HasSpan
        => Span.IsEmpty is false;

    /// <summary>
    /// 判断当前诊断是否为错误级别。
    /// </summary>
    public bool IsError
        => Severity is DiagnosticSeverity.Error;
}
