
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;
/// <summary>
/// 表示整个 INI 文件的根语法节点。
/// 包含所有节（包括虚拟节）。
/// </summary>
public sealed class DocumentSyntaxNode : SyntaxNode
{
    private readonly Lazy<IReadOnlyList<SectionSyntaxNode>> _sections;

    /// <summary>
    /// 初始化一个新的 <see cref="DocumentSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    internal DocumentSyntaxNode(DocumentNode green)
        : base(green, null)
    {
        Green = green;
        _sections = new(() => [.. green.Sections.Select(s => new SectionSyntaxNode(s, this))]);
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new DocumentNode Green { get; }

    /// <summary>
    /// 获取文件中的所有节（包括虚拟节）
    /// </summary>
    public IReadOnlyList<SectionSyntaxNode> Sections => _sections.Value;

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
