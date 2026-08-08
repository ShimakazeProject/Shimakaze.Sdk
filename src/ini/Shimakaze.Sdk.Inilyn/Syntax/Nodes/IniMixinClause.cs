namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// Mixin 子句：冒号 + 引用列表。
/// </summary>
    /// <param name="colon">冒号 token。</param>
    /// <param name="references">Mixin 引用列表。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniMixinClause(
    IniSyntaxToken colon,
    IReadOnlyList<IniMixinReference> references,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => IniSyntaxKind.MixinReferenceList;

    /// <summary>
    /// 冒号 token。
    /// </summary>
    public IniSyntaxToken Colon => colon;

    /// <summary>
    /// Mixin 引用列表。
    /// </summary>
    public IReadOnlyList<IniMixinReference> References => references;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [Colon, .. References];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniMixinClause clause)
        {
            return false;
        }

        return Start == clause.Start
            && End == clause.End
            && References.SequenceEqual(clause.References);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Start, End);
}
