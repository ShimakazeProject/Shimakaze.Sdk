namespace Shimakaze.Sdk.Inilyn.Analysis;

/// <summary>
/// 分析器管道的最终输出结果。
/// </summary>
public sealed class AnalyzerResult
{
    /// <summary>
    /// 分析过程中产生的诊断信息。
    /// </summary>
    public List<AnalyzerDiagnostic> Diagnostics { get; } = [];

    /// <summary>
    /// 分析是否成功完成（无错误诊断）。
    /// </summary>
    public bool IsSuccess => Diagnostics.TrueForAll(d => d.Severity != AnalyzerDiagnosticSeverity.Error);
}

/// <summary>
/// 分析器管道产生的诊断信息。
/// </summary>
/// <param name="Code">诊断代码。</param>
/// <param name="Message">诊断消息。</param>
/// <param name="Severity">严重级别。</param>
/// <param name="Range">源位置范围。</param>
public sealed record class AnalyzerDiagnostic(
    string Code,
    string Message,
    AnalyzerDiagnosticSeverity Severity,
    in AnalyzerRange Range);

/// <summary>
/// 诊断信息的严重级别。
/// </summary>
public enum AnalyzerDiagnosticSeverity
{
    /// <summary>
    /// 提示。
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

/// <summary>
/// 源文件中的位置范围。
/// </summary>
public readonly record struct AnalyzerRange
{
    /// <summary>
    /// 起始位置。
    /// </summary>
    public AnalyzerPosition Start { get; init; }

    /// <summary>
    /// 结束位置。
    /// </summary>
    public AnalyzerPosition End { get; init; }
}

/// <summary>
/// 源文件中的位置。
/// </summary>
public readonly record struct AnalyzerPosition
{
    /// <summary>
    /// 行号（零基）。
    /// </summary>
    public uint Line { get; init; }

    /// <summary>
    /// 列号（零基）。
    /// </summary>
    public uint Character { get; init; }
}
