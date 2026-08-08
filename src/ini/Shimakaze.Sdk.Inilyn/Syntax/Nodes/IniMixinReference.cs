namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// Mixin 引用：[ref]。
/// </summary>
    /// <param name="leadingTrivia">前导 trivia 列表。</param>
    /// <param name="leftBracket">左方括号 token。</param>
    /// <param name="name">引用的节名 token。</param>
    /// <param name="rightBracket">右方括号 token。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniMixinReference(
    IReadOnlyList<IniSyntaxNode> leadingTrivia,
    IniSyntaxToken leftBracket,
    IniSyntaxToken name,
    IniSyntaxToken rightBracket,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => IniSyntaxKind.MixinReference;

    /// <summary>
    /// 前导 trivia 列表。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> LeadingTrivia => leadingTrivia;

    /// <summary>
    /// 左方括号 token。
    /// </summary>
    public IniSyntaxToken LeftBracket => leftBracket;

    /// <summary>
    /// 引用的节名 token。
    /// </summary>
    public IniSyntaxToken Name => name;

    /// <summary>
    /// 右方括号 token。
    /// </summary>
    public IniSyntaxToken RightBracket => rightBracket;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [.. LeadingTrivia, LeftBracket, Name, RightBracket];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniMixinReference reference)
        {
            return false;
        }

        return Start == reference.Start
            && End == reference.End
            && Equals(Name, reference.Name);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Start, End, Name);
}
