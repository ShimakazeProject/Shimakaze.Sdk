
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示一个 INI 键值对的语法节点。
/// 包含 Key、可选的 Value 和可选的 Comment。
/// </summary>
public sealed class KeyValuePairSyntaxNode : SyntaxNode
{
    /// <summary>
    /// 初始化一个新的 <see cref="KeyValuePairSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal KeyValuePairSyntaxNode(KeyValuePairNode green, SyntaxNode? parent)
        : base(green, parent)
    {
        Green = green;
        Key = new(Green.Key, this);
        if (Green.Value is not null)
            Value = new(Green.Value, this);
        if (Green.Comment is not null)
            Comment = new(Green.Comment, this);
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new KeyValuePairNode Green { get; }

    /// <summary>
    /// 获取键部分的语法节点。
    /// </summary>
    public KeySyntaxNode Key { get; }

    /// <summary>
    /// 获取值部分的语法节点（可能为 null）。
    /// </summary>
    public ValueSyntaxNode? Value { get; }

    /// <summary>
    /// 获取注释部分的语法节点（可能为 null）。
    /// </summary>
    public CommentSyntaxNode? Comment { get; }

    /// <summary>
    /// 获取当前节点的所有直接子节点。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Key;
        if (Value is not null)
            yield return Value;
        if (Comment is not null)
            yield return Comment;
    }

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
