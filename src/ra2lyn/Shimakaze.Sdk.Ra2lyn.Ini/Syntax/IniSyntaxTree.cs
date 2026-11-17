using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 一次语法分析的结果。
/// </summary>
public sealed class IniSyntaxTree
{
    /// <summary>
    /// 文档根节点。
    /// </summary>
    public required IniDocumentNode Root { get; init; }

    /// <summary>
    /// 语法分析产生的诊断，按产生顺序排列。
    /// </summary>
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }
}
