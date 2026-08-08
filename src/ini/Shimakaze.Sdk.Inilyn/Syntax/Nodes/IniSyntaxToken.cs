namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// 语法树中的 token 节点（对词法 token 的轻量包装）。
/// </summary>
    /// <param name="kind">语法 token 类型。</param>
    /// <param name="text">词素文本。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniSyntaxToken(
    IniSyntaxKind kind,
    string text,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => kind;

    /// <summary>
    /// 词素文本。
    /// </summary>
    public string Text => text;

    /// <summary>
    /// 是否为缺失 token（错误恢复时插入）。
    /// </summary>
    public bool IsMissing => Text.Length == 0 && Kind != IniSyntaxKind.EndOfFileToken;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniSyntaxToken token)
        {
            return false;
        }

        return Kind == token.Kind
            && Text == token.Text
            && Start == token.Start
            && End == token.End;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Text, Start, End);
}
