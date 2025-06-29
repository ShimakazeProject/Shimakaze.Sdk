
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;
/// <summary>
/// 表示整个 INI 文件的根语法节点。
/// 包含所有节（包括虚拟节）。
/// </summary>
public sealed class DocumentSyntaxNode : SyntaxNode
{
    /// <summary>
    /// 初始化一个新的 <see cref="DocumentSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal DocumentSyntaxNode(DocumentNode green, SyntaxNode? parent)
        : base(green, parent)
    {
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new DocumentNode Green => (DocumentNode)base.Green;

    /// <summary>
    /// 获取文件中的所有节（包括虚拟节）
    /// </summary>
    public IReadOnlyList<SectionSyntaxNode> Sections => Green.Sections
        .Select(s => new SectionSyntaxNode(s, this))
        .ToList();

    /// <summary>
    /// 获取当前节点的所有直接子节点。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public override IEnumerable<SyntaxNode> GetChildren() => Sections;

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
