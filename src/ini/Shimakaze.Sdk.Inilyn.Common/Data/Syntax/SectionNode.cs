namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 段落节点，例如 <c>[SectionName] : [Base1] , [Base2]</c>。
/// </summary>
public sealed class SectionNode : SyntaxNode
{
    /// <summary>
    /// 段落名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 是否存在左方括号 <c>[</c>。
    /// </summary>
    public bool HasOpenBracket { get; set; }

    /// <summary>
    /// 是否存在右方括号 <c>]</c>。
    /// </summary>
    public bool HasCloseBracket { get; set; }

    /// <summary>
    /// 继承的段落列表（冒号或逗号分隔的名称）。
    /// </summary>
    public List<SectionInheritance> Inheritances { get; set; } = [];

    /// <summary>
    /// 属于该段落的键值对列表。
    /// </summary>
    public List<KeyValuePairNode> KeyValues { get; set; } = [];
}
