namespace Shimakaze.Sdk.Inilyn.Syntax.Nodes;

/// <summary>
/// 编译单元（语法树根节点）。
/// </summary>
    /// <param name="entries">顶层条目列表。</param>
    /// <param name="endOfFile">文件结束 token。</param>
    /// <param name="start">起始位置。</param>
    /// <param name="end">结束位置。</param>
public sealed class IniCompilationUnit(
    IReadOnlyList<IniSyntaxNode> entries,
    IniSyntaxToken endOfFile,
    int start,
    int end
) : IniSyntaxNode(start, end)
{
    /// <inheritdoc />
    public override IniSyntaxKind Kind => IniSyntaxKind.CompilationUnit;

    /// <summary>
    /// 顶层条目列表。
    /// </summary>
    public IReadOnlyList<IniSyntaxNode> Entries => entries;

    /// <summary>
    /// 文件结束 token。
    /// </summary>
    public IniSyntaxToken EndOfFile => endOfFile;

    /// <inheritdoc />
    public override IReadOnlyList<IniSyntaxNode> ChildNodes => entries;

    /// <inheritdoc />
    public override bool Equals(IniSyntaxNode? other)
    {
        if (other is not IniCompilationUnit unit)
        {
            return false;
        }

        return Start == unit.Start
            && End == unit.End
            && Entries.SequenceEqual(unit.Entries);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Kind, Start, End);
}
