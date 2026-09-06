using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Data.Syntax;

/// <summary>
/// 诊断信息，记录语法分析过程中发现的问题。
/// </summary>
public sealed class IniDiagnostic
{
    /// <summary>
    /// 诊断唯一标识。
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
    /// 严重级别。
    /// </summary>
    public Shimakaze.LanguageServerProtocol.Model.DiagnosticSeverity Severity { get; set; }

    /// <summary>
    /// 诊断代码，例如 <c>INI001</c>。
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 诊断消息。
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 诊断在源文件中的范围。
    /// </summary>
    public LsRange Range { get; set; } = null!;

    /// <summary>
    /// 诊断来源（如 <c>Syntax</c>、<c>Semantic</c>）。
    /// </summary>
    public string? Source { get; set; }
}
