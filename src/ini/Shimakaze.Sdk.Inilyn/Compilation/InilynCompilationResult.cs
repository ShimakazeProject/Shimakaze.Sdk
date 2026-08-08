using Shimakaze.Sdk.Inilyn.SourceMapping;
using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Compilation;

/// <summary>
/// 编译结果。
/// </summary>
public sealed class InilynCompilationResult
{
    /// <summary>
    /// 输出文件映射（键为文件名，值为生成的 INI 文本）。
    /// </summary>
    public IReadOnlyDictionary<string, string> OutputFiles { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// 源映射。
    /// </summary>
    public SourceMap SourceMap { get; init; } = new();

    /// <summary>
    /// 编译过程中收集的所有诊断信息。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = [];

    /// <summary>
    /// 编译是否成功（无错误级诊断）。
    /// </summary>
    public bool Success => !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
