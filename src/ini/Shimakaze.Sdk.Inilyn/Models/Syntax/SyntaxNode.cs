
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 所有 Red Tree（语法红树）节点的公共基类。
/// 提供语法节点的通用属性和方法，支持访问者模式（Visitor Pattern）和语法树导航。
/// </summary>
public abstract class SyntaxNode
{
    /// <summary>
    /// 对应的绿树节点（Green Node），用于存储原始语法信息和位置范围。
    /// </summary>
    internal readonly GreenNode Green;

    /// <summary>
    /// 获取当前节点的父节点。根节点的父节点为 null。
    /// </summary>
    public SyntaxNode? Parent { get; }

    /// <summary>
    /// 获取当前语法节点的种类（类型），例如 Key、Value、Section 等。
    /// </summary>
    public SyntaxKind Kind => Green.Kind;

    /// <summary>
    /// 获取当前语法节点在源文件中的位置范围（行号、列号、长度等）。
    /// 用于支持 LSP（Language Server Protocol）等功能。
    /// </summary>
    public LSPRange Range => Green.Range;

    /// <summary>
    /// 指示当前语法节点是否为叶子节点（即不包含子节点的终端节点）。
    /// 默认返回 false，派生类可重写此属性以表示自身为叶子节点。
    /// </summary>
    public virtual bool IsLeaf => false;

    /// <summary>
    /// 初始化一个新的 <see cref="SyntaxNode"/> 实例。
    /// </summary>
    /// <param name="green">对应的绿树节点。</param>
    /// <param name="parent">当前节点的父节点，若为根节点则为 null。</param>
    internal SyntaxNode(GreenNode green, SyntaxNode? parent)
    {
        Green = green;
        Parent = parent;
    }

    /// <summary>
    /// 获取当前节点的所有直接子节点。
    /// 默认实现为空枚举，派生类可重写此方法以提供子节点集合。
    /// </summary>
    /// <returns>当前节点的子节点序列。</returns>
    public virtual IEnumerable<SyntaxNode> GetChildren()
    {
        yield break;
    }

    /// <summary>
    /// 接受一个 <see cref="ISyntaxVisitor"/> 来访问该节点。
    /// 用于实现访问者模式，便于对语法树进行遍历或分析。
    /// </summary>
    /// <param name="visitor">要执行访问操作的语法访问器。</param>
    public abstract void Accept(ISyntaxVisitor visitor);
}
