
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示一个完整的 INI 节的语法节点。
/// 包含文档注释、节头和节内容。
/// </summary>
public sealed class SectionSyntaxNode : SyntaxNode
{
    /// <summary>
    /// 初始化一个新的 <see cref="SectionSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal SectionSyntaxNode(SectionNode green, SyntaxNode? parent)
        : base(green, parent)
    {
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new SectionNode Green => (SectionNode)base.Green;

    /// <summary>
    /// 获取当前节的文档注释块节点（可能为 null）。
    /// </summary>
    public DocumentCommentBlockSyntaxNode? DocumentComment => Green.DocumentComment is not null
        ? new(Green.DocumentComment, this)
        : null;

    /// <summary>
    /// 获取当前节的节头语法节点（可能为 null）。
    /// </summary>
    public SectionHeaderSyntaxNode? SectionHeader => Green.SectionHeader is not null
        ? new(Green.SectionHeader, this)
        : null;

    /// <summary>
    /// 获取当前节的数据部分语法节点。
    /// </summary>
    public SectionDataSyntaxNode SectionData => new(Green.SectionData, this);

    /// <summary>
    /// 获取当前节点的所有直接子节点。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (DocumentComment is not null)
            yield return DocumentComment;
        if (SectionHeader is not null)
            yield return SectionHeader;
        yield return SectionData;
    }

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
