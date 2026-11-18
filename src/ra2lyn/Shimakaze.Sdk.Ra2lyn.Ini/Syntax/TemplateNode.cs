using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 模板节节点，对应 <c>[$Name] : where Expr</c> 或无条件的 <c>[$Name]</c>（TODO.md §4.2）。
/// </summary>
/// <remarks>
/// <para>
/// 模板节名以 <c>$</c> 开头；本节点保存的 <see cref="Name"/> 已经去掉了 <c>$</c> 前缀。
/// <see cref="WhereExpression"/> 为 <see langword="null"/> 表示无条件模板，对当前 Group 范围内
/// 所有节生效（由后续 TemplateExpansionPass 处理，本步骤不做求值与应用）。
/// </para>
/// </remarks>
public sealed class TemplateNode : SyntaxNode
{
    /// <summary>
    /// 模板名，不含前导 <c>$</c>，首尾空白已裁剪。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// <c>where</c> 关键字之后的原始表达式文本；<see langword="null"/> 表示无条件模板。
    /// </summary>
    public string? WhereExpression { get; init; }

    /// <summary>
    /// 属于本模板的键值对列表，顺序与源文件一致。
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
    /// 模板名（含 <c>$</c> 前缀）在源文件中的范围（已裁剪首尾空白）；名为空时为零宽范围。
    /// </summary>
    public SourceSpan NameSpan { get; init; }

    /// <summary>
    /// <see cref="WhereExpression"/> 占据的范围；无条件模板为零宽范围。
    /// </summary>
    public SourceSpan WhereSpan { get; init; }

    /// <summary>
    /// 节头跨度：从可用的 <c>[</c> 开始，到 <c>]</c> 或 where 表达式末尾为止。
    /// </summary>
    public SourceSpan HeaderSpan { get; init; }
}
