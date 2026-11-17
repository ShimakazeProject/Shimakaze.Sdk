using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// INI 文档根节点。
/// </summary>
/// <remarks>
/// 文档按“自由记号、条目、自由记号”三段组织：文件头与文件尾的注释、空白、换行都保留在
/// <see cref="LeadingTrivia"/> 与 <see cref="TrailingTrivia"/> 中。条目之间的自由记号作为后一个条目的
/// 前导记号出现：节头行之后的注释与换行先挂到节节点的 <see cref="SectionNode.TrailingTrivia"/>，
/// 再进入下一个条目的前导记号。
/// </remarks>
public sealed class IniDocumentNode : SyntaxNode
{
    /// <summary>
    /// 文件头自由记号（第一个条目之前的全部注释、空白与换行）。
    /// </summary>
    public required IReadOnlyList<Trivia> LeadingTrivia { get; init; }

    /// <summary>
    /// 顶层条目，元素为 <see cref="SectionNode"/> 或 <see cref="KeyValueNode"/>，顺序与源文件一致。
    /// </summary>
    public required IReadOnlyList<SyntaxNode> Entries { get; init; }

    /// <summary>
    /// 文件尾自由记号（最后一个条目之后的全部注释、空白与换行）。
    /// </summary>
    public required IReadOnlyList<Trivia> TrailingTrivia { get; init; }
}
