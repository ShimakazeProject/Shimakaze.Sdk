
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示一个由多个文档注释组成的块级语法节点。
/// </summary>
public sealed class DocumentCommentBlockSyntaxNode : SyntaxNode
{
    private readonly Lazy<IReadOnlyList<DocumentCommentSyntaxNode>> _comments;
    /// <summary>
    /// 初始化一个新的 <see cref="DocumentCommentBlockSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal DocumentCommentBlockSyntaxNode(DocumentCommentBlockNode green, SyntaxNode? parent)
        : base(green, parent)
    {
        Green = green;
        _comments = new(() => [.. green.Comments.Select(i => new DocumentCommentSyntaxNode(i, this))]);
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new DocumentCommentBlockNode Green { get; }

    /// <summary>
    /// 获取该注释块中的所有文档注释节点。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public override IEnumerable<DocumentCommentSyntaxNode> GetChildren() => _comments.Value;

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
