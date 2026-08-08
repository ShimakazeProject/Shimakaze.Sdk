namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// 诊断信息。
/// </summary>
/// <param name="Severity">严重级别。</param>
/// <param name="Code">诊断代码。</param>
/// <param name="Message">诊断消息。</param>
/// <param name="Line">起始行号（1-based，0 表示无位置信息）。</param>
/// <param name="Column">起始列号（1-based）。</param>
/// <param name="EndLine">结束行号（1-based，0 表示无结束位置）。</param>
/// <param name="EndColumn">结束列号（1-based）。</param>
/// <param name="FilePath">文件路径。</param>
public sealed record class Diagnostic(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    int Line = 0,
    int Column = 0,
    int EndLine = 0,
    int EndColumn = 0,
    string? FilePath = null)
{
    /// <summary>
    /// 从描述符创建诊断实例。
    /// </summary>
    /// <param name="descriptor">诊断描述符。</param>
    /// <param name="line">起始行号（1-based）。</param>
    /// <param name="column">起始列号（1-based）。</param>
    /// <param name="endLine">结束行号（1-based）。</param>
    /// <param name="endColumn">结束列号（1-based）。</param>
    /// <param name="filePath">文件路径。</param>
    /// <param name="messageArgs">消息格式参数。</param>
    /// <returns>诊断实例。</returns>
    public static Diagnostic Create(
        DiagnosticDescriptor descriptor,
        int line = 0,
        int column = 0,
        int endLine = 0,
        int endColumn = 0,
        string? filePath = null,
        params object[]? messageArgs)
    {
        return new Diagnostic(
            descriptor.DefaultSeverity,
            descriptor.Id,
            string.Format(descriptor.MessageFormat, messageArgs ?? []),
            line,
            column,
            endLine,
            endColumn,
            filePath);
    }
}

/// <summary>
/// 诊断严重级别。
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// 信息。
    /// </summary>
    Info,

    /// <summary>
    /// 警告。
    /// </summary>
    Warning,

    /// <summary>
    /// 错误。
    /// </summary>
    Error,
}
