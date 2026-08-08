namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// Trivia 节点（注释、空白、换行）。
/// </summary>
    /// <param name="kind">Trivia 类型。</param>
    /// <param name="text">Trivia 文本。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniTriviaNode(
    IniSyntaxKind kind,
    string text,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => kind;

    /// <summary>
    /// Trivia 文本。
    /// </summary>
    public string Text => text;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniTriviaNode trivia)
        {
            return false;
        }

        return Kind == trivia.Kind
            && Text == trivia.Text
            && Start == trivia.Start
            && End == trivia.End;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Text, Start, End);
}
