namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// 节声明：[name] 或 [name] : [ref1], [ref2]。
/// </summary>
    /// <param name="leadingTrivia">前导 trivia 列表。</param>
    /// <param name="leftBracket">左方括号 token。</param>
    /// <param name="name">节名 token。</param>
    /// <param name="rightBracket">右方括号 token。</param>
    /// <param name="mixinClause">可选的 Mixin 子句（冒号 + 引用列表）。</param>
    /// <param name="trailingTrivia">尾随 trivia 列表（含换行）。</param>
    /// <param name="children">子节点（节内的键值对和条目）。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniSectionDecl(
    IReadOnlyList<IniSyntaxNode> leadingTrivia,
    IniSyntaxToken leftBracket,
    IniSyntaxToken name,
    IniSyntaxToken rightBracket,
    IniMixinClause? mixinClause,
    IReadOnlyList<IniSyntaxNode> trailingTrivia,
    IReadOnlyList<IniSyntaxNode> children,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => IniSyntaxKind.SectionDeclaration;

    /// <summary>
    /// 前导 trivia 列表。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> LeadingTrivia => leadingTrivia;

    /// <summary>
    /// 左方括号 token。
    /// </summary>
    public IniSyntaxToken LeftBracket => leftBracket;

    /// <summary>
    /// 节名 token。
    /// </summary>
    public IniSyntaxToken Name => name;

    /// <summary>
    /// 右方括号 token。
    /// </summary>
    public IniSyntaxToken RightBracket => rightBracket;

    /// <summary>
    /// 可选的 Mixin 子句（冒号 + 引用列表）。
    /// </summary>
    public IniMixinClause? MixinClause => mixinClause;

    /// <summary>
    /// 尾随 trivia 列表（含换行）。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> TrailingTrivia => trailingTrivia;

    /// <summary>
    /// 子节点（节内的键值对和条目）。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> Children => children;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [.. LeadingTrivia, LeftBracket, Name, RightBracket, .. (MixinClause is not null ? (IReadOnlyList<IniSyntaxNode>)[MixinClause] : []), .. TrailingTrivia, .. Children];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniSectionDecl section)
        {
            return false;
        }

        return Start == section.Start
            && End == section.End
            && Equals(Name, section.Name)
            && Equals(MixinClause, section.MixinClause);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Start, End, Name);
}
