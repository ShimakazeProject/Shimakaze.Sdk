using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 无关记号（注释、空白、换行）。
/// </summary>
public sealed class TriviaToken
{
    /// <summary>
    /// 记号唯一标识。
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
    /// 记号类型。
    /// </summary>
    public TriviaKind Kind { get; set; }

    /// <summary>
    /// 原始文本。
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 记号在源文件中的范围。
    /// </summary>
    public LsRange Range { get; set; } = null!;

    /// <summary>
    /// 依附的语法节点标识。自由记号为 <see langword="null"/>。
    /// </summary>
    public Guid? AttachedToNodeId { get; set; }

    /// <summary>
    /// 是否为前导记号（<see langword="true"/>）或尾随记号（<see langword="false"/>）。
    /// </summary>
    public bool IsLeading { get; set; }
}
