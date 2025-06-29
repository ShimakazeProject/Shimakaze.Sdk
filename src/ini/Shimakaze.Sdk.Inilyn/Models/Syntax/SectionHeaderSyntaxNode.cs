
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示一个 INI 节头的语法节点，例如：
/// [section]
/// [derived] : [base]
/// </summary>
public sealed class SectionHeaderSyntaxNode : SyntaxNode
{
    /// <summary>
    /// 初始化一个新的 <see cref="SectionHeaderSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal SectionHeaderSyntaxNode(SectionHeaderNode green, SyntaxNode? parent)
        : base(green, parent)
    {
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new SectionHeaderNode Green => (SectionHeaderNode)base.Green;

    /// <summary>
    /// 获取当前节的名称语法节点。
    /// </summary>
    public SectionNameSyntaxNode SectionName => new(Green.SectionName, this);

    /// <summary>
    /// 获取当前节的继承节名称语法节点（可能为 null）。
    /// </summary>
    public InheritSectionNameSyntaxNode? InheritSectionName => Green.InheritSectionName is not null
        ? new(Green.InheritSectionName, this)
        : null;

    /// <summary>
    /// 获取当前节头中的注释语法节点（可能为 null）。
    /// </summary>
    public CommentSyntaxNode? Comment => Green.Comment is not null
        ? new(Green.Comment, this)
        : null;

    /// <summary>
    /// 获取当前节点的所有直接子节点。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return SectionName;
        if (InheritSectionName is not null)
            yield return InheritSectionName;
        if (Comment is not null)
            yield return Comment;
    }

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
