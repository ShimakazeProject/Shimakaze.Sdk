
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;
using Shimakaze.Sdk.Inilyn.Models.Token;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示一个 INI 键（Key）的语法节点。
/// 该节点通常出现在键值对中，如 Key=Value 中的 Key 部分。
/// </summary>
public sealed class KeySyntaxNode : SyntaxNode
{
    /// <summary>
    /// 初始化一个新的 <see cref="KeySyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal KeySyntaxNode(KeyNode green, SyntaxNode? parent)
        : base(green, parent)
    {
    }

    /// <summary>
    /// 获取与此红树节点关联的绿树节点。
    /// </summary>
    internal new KeyNode Green => (KeyNode)base.Green;

    /// <summary>
    /// 获取当前键的词法单元（Token），包含原始文本及其位置信息。
    /// </summary>
    public IniToken Token => Green.Token;

    /// <summary>
    /// 获取一个值，指示此节点是否为叶子节点（即不包含子节点的终端节点）。
    /// 对于 <see cref="KeySyntaxNode"/>，始终返回 true。
    /// </summary>
    public override bool IsLeaf => true;

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public override void Accept(ISyntaxVisitor visitor) => visitor.Visit(this);
}
