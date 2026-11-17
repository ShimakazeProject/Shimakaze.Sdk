using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 节节点，例如 <c>[A]:[B],[C]</c> 及其下属的键值对。
/// </summary>
public sealed class SectionNode : SyntaxNode
{
    /// <summary>
    /// 节名，不含方括号，首尾空白已裁剪。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 是否存在左方括号 <c>[</c>。
    /// </summary>
    public bool HasOpenBracket { get; init; }

    /// <summary>
    /// 是否存在右方括号 <c>]</c>。
    /// </summary>
    public bool HasCloseBracket { get; init; }

    /// <summary>
    /// 继承项列表，顺序与源文件一致。
    /// </summary>
    public required IReadOnlyList<SectionInheritance> Inheritances { get; init; }

    /// <summary>
    /// 属于本节的键值对列表，顺序与源文件一致。
    /// </summary>
    public required IReadOnlyList<KeyValueNode> KeyValues { get; init; }

    /// <summary>
    /// 前导自由记号：节头行之前的注释、空白与换行。
    /// </summary>
    public required IReadOnlyList<Trivia> LeadingTrivia { get; init; }

    /// <summary>
    /// 尾随自由记号：节头行的剩余部分（含换行）。
    /// </summary>
    public required IReadOnlyList<Trivia> TrailingTrivia { get; init; }

    /// <summary>
    /// 节名在源文件中的范围（已裁剪首尾空白）；节名为空时为零宽范围。
    /// </summary>
    public SourceSpan NameSpan { get; init; }

    /// <summary>
    /// 节头跨度：从可用的 <c>[</c> 开始，到可用的 <c>]</c> 或继承列表末尾为止。
    /// </summary>
    /// <remarks>
    /// 该范围可能含有内部空白甚至注释（例如 <c>[A B]</c> 中的空格），不保证是连续子串；
    /// 需要连续位置时请使用 <see cref="NameSpan"/>。
    /// </remarks>
    public SourceSpan HeaderSpan { get; init; }
}
