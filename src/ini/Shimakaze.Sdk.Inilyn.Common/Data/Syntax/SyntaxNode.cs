using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 语法树节点基类。
/// </summary>
public abstract class SyntaxNode
{
    /// <summary>
    /// 节点唯一标识。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所属文档标识。
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// 所属文档。
    /// </summary>
    public IniDocument Document { get; set; } = null!;

    /// <summary>
    /// 在父节点内的排列顺序。
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 节点在源文件中的范围。
    /// </summary>
    public LsRange Range { get; set; } = null!;
}
