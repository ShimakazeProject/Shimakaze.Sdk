using LsDiagnosticSeverity = Shimakaze.LanguageServerProtocol.Model.DiagnosticSeverity;

namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// 诊断描述符：描述一种诊断的静态元数据。
/// </summary>
/// <param name="id">诊断代码（如 <c>INI001</c>）。</param>
/// <param name="title">简短标题。</param>
/// <param name="messageFormat">消息格式（<c>{0}</c>/<c>{1}</c> 占位符由诊断参数填充）。</param>
/// <param name="category">类别（如 <c>Syntax</c>、<c>Semantic</c>）。</param>
/// <param name="defaultSeverity">默认严重级别。</param>
public sealed class DiagnosticDescriptor(
    string id,
    string title,
    string messageFormat,
    string category,
    LsDiagnosticSeverity defaultSeverity)
{
    /// <summary>
    /// 诊断代码。
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// 简短标题。
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// 消息格式。
    /// </summary>
    public string MessageFormat { get; } = messageFormat;

    /// <summary>
    /// 类别。
    /// </summary>
    public string Category { get; } = category;

    /// <summary>
    /// 默认严重级别。
    /// </summary>
    public LsDiagnosticSeverity DefaultSeverity { get; } = defaultSeverity;
}
