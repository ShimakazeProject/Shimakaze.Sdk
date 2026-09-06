using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 段落继承项，表示段落头中通过 <c>:</c> 或 <c>,</c> 引用的基类名称。
/// </summary>
public sealed class SectionInheritance
{
    /// <summary>
    /// 继承项唯一标识。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所属段落节点标识。
    /// </summary>
    public Guid SectionNodeId { get; set; }

    /// <summary>
    /// 所属段落节点。
    /// </summary>
    public SectionNode SectionNode { get; set; } = null!;

    /// <summary>
    /// 在继承列表中的排列顺序。
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 继承的名称。
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
    /// 前置分隔符类型。
    /// </summary>
    public SectionSeparator Separator { get; set; }

    /// <summary>
    /// 该继承项在源文件中的范围。
    /// </summary>
    public LsRange Range { get; set; } = null!;
}
