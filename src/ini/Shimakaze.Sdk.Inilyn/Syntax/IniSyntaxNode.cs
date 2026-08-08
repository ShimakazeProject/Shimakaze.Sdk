namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// INI 语法节点的抽象基类。
/// </summary>
/// <remarks>
/// 所有语法节点不可变，支持结构相等。每个节点记录其在源文本中的起止偏移。
/// </remarks>
public abstract class IniSyntaxNode(int start, int end) : IEquatable<IniSyntaxNode>
{
    /// <summary>
    /// 语法节点类型。
    /// </summary>
    public abstract IniSyntaxKind Kind { get; }

    /// <summary>
    /// 节点在源文本中的起始位置（从 0 开始）。
    /// </summary>
    public int Start { get; } = start;

    /// <summary>
    /// 节点在源文本中的结束位置（从 0 开始，不含）。
    /// </summary>
    public int End { get; } = end;

    /// <summary>
    /// 节点跨度（字符数）。
    /// </summary>
    public int Length => End - Start;

    /// <summary>
    /// 获取子节点列表。
    /// </summary>
    public abstract IReadOnlyList<IniSyntaxNode> ChildNodes { get; }

    /// <inheritdoc />
    public abstract bool Equals(IniSyntaxNode? other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as IniSyntaxNode);

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <summary>
    /// 判断两个语法节点是否相等。
    /// </summary>
    public static bool operator ==(IniSyntaxNode? left, IniSyntaxNode? right) => Equals(left, right);

    /// <summary>
    /// 判断两个语法节点是否不相等。
    /// </summary>
    public static bool operator !=(IniSyntaxNode? left, IniSyntaxNode? right) => !Equals(left, right);
}
