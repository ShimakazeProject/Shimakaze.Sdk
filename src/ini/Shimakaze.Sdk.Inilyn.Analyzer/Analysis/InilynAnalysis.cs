using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Analyzer.Analysis;

/// <summary>
/// 分析结果。
/// </summary>
/// <remarks>
/// 创建一个分析结果。
/// </remarks>
/// <param name="sections">节分析列表。</param>
/// <param name="diagnostics">诊断信息。</param>
public sealed class InilynAnalysis(IReadOnlyList<InilynSectionAnalysis> sections, IReadOnlyList<Diagnostic> diagnostics)
{
    /// <summary>
    /// 所有节的分析结果。
    /// </summary>
    public IReadOnlyList<InilynSectionAnalysis> Sections { get; } = sections;

    /// <summary>
    /// 诊断信息。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; } = diagnostics;

    /// <summary>
    /// 是否存在错误级诊断。
    /// </summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// 可被 TreeShaking 移除的节（不可达的实体节）。
    /// </summary>
    public IReadOnlyList<InilynSectionAnalysis> TreeShakeable => [.. Sections.Where(s => !s.IsReachable)];
}
