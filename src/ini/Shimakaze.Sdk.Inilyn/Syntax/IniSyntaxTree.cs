using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// INI 语法树。
/// </summary>
/// <remarks>
/// 持有源文本、根节点和解析过程中收集的诊断信息。
/// </remarks>
/// <param name="sourceText">源文本。</param>
/// <param name="root">根节点（编译单元）。</param>
/// <param name="diagnostics">诊断信息列表。</param>
public sealed class IniSyntaxTree(SourceText sourceText, IniSyntaxNode root, IReadOnlyList<Diagnostic> diagnostics)
{
    /// <summary>
    /// 源文本。
    /// </summary>
    public SourceText SourceText { get; } = sourceText;

    /// <summary>
    /// 根节点（编译单元）。
    /// </summary>
    public IniSyntaxNode Root { get; } = root;

    /// <summary>
    /// 解析过程中收集的诊断信息。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; } = diagnostics;

    /// <summary>
    /// 是否存在错误级别的诊断。
    /// </summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
