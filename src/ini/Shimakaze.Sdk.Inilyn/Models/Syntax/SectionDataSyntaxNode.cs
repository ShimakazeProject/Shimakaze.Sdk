
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示一个节的数据部分，包含键值对、注释和编译器指令。
/// </summary>
public sealed class SectionDataSyntaxNode : SyntaxNode
{
    private readonly Lazy<IReadOnlyList<SyntaxNode>> _items;
    /// <summary>
    /// 初始化一个新的 <see cref="SectionDataSyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal SectionDataSyntaxNode(SectionDataNode green, SyntaxNode? parent)
        : base(green, parent)
    {
        Green = green;
        _items = new(() => [.. Green.Items.Select(CreateRedNode).OfType<SyntaxNode>()]);
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new SectionDataNode Green { get; }

    /// <summary>
    /// 获取所有数据项（键值对、注释、编译器指令等）
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="KeyValuePairSyntaxNode"/> 键值对</item>
    /// <item><see cref="CommentSyntaxNode"/> 普通注释</item>
    /// <item><see cref="DocumentCommentSyntaxNode"/> 意外写的文档注释 作为普通注释处理</item>
    /// <!--<item><see cref="ErrorNode"/> 语法错误</item>-->
    /// </list>
    /// </remarks>
    public IReadOnlyList<SyntaxNode> Items => _items.Value;

    /// <summary>
    /// 获取当前节点的所有直接子节点。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public override IEnumerable<SyntaxNode> GetChildren() => Items;

    private SyntaxNode? CreateRedNode(GreenNode green) => green switch
    {
        KeyValuePairNode kv => new KeyValuePairSyntaxNode(kv, this),
        CommentNode c => new CommentSyntaxNode(c, this),
        DocumentCommentNode dc => new DocumentCommentSyntaxNode(dc, this),
        _ => null,
    };

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
