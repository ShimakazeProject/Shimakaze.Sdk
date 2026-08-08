namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// 键值对条目：key = value。
/// </summary>
    /// <param name="leadingTrivia">前导 trivia 列表。</param>
    /// <param name="key">键 token。</param>
    /// <param name="equalSign">等号 token。</param>
    /// <param name="value">值 token。</param>
    /// <param name="trailingTrivia">尾随 trivia 列表（含换行）。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniKeyValueEntry(
    IReadOnlyList<IniSyntaxNode> leadingTrivia,
    IniSyntaxToken key,
    IniSyntaxToken equalSign,
    IniSyntaxToken value,
    IReadOnlyList<IniSyntaxNode> trailingTrivia,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => IniSyntaxKind.KeyValueEntry;

    /// <summary>
    /// 前导 trivia 列表。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> LeadingTrivia => leadingTrivia;

    /// <summary>
    /// 键 token。
    /// </summary>
    public IniSyntaxToken Key => key;

    /// <summary>
    /// 等号 token。
    /// </summary>
    public IniSyntaxToken EqualSign => equalSign;

    /// <summary>
    /// 值 token。
    /// </summary>
    public IniSyntaxToken Value => value;

    /// <summary>
    /// 尾随 trivia 列表（含换行）。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> TrailingTrivia => trailingTrivia;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => [.. LeadingTrivia, Key, EqualSign, Value, .. TrailingTrivia];

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniKeyValueEntry entry)
        {
            return false;
        }

        return Start == entry.Start
            && End == entry.End
            && Key.Equals(entry.Key)
            && Value.Equals(entry.Value);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Start, End, Key, Value);
}
