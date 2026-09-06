using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Data.Lexer;

/// <summary>
/// 词法记号持久化实体。
/// </summary>
public sealed class IniToken
{
    /// <summary>
    /// 主键。
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所属文档的 Id。
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// 记号在文档中的顺序（从 0 开始）。
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 记号类型。
    /// </summary>
    public IniTokenType Type { get; set; }

    /// <summary>
    /// 记号位置（零基）。
    /// </summary>
    public LsRange Position { get; set; } = null!;

    /// <summary>
    /// 记号的词素文本。
    /// </summary>
    public string Text { get; set; } = string.Empty;
}