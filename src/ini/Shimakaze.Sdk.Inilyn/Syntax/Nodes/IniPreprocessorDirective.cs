namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// 预处理指令节点。
/// </summary>
    /// <param name="token">指令 token（包含完整指令文本）。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniPreprocessorDirective(
    IniSyntaxToken token,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => IniSyntaxKind.PreprocessorDirective;

    /// <summary>
    /// 指令 token（文本形如 <c>#if DEBUG</c>、<c>#region</c> 等）。
    /// </summary>
    public IniSyntaxToken Token => token;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [Token];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniPreprocessorDirective directive)
        {
            return false;
        }

        return Start == directive.Start
            && End == directive.End
            && Equals(Token, directive.Token);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Start, End, Token);
}
