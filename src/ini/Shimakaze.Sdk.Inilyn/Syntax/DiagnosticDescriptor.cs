namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// 诊断描述符：描述一种诊断的静态元数据（仿 Roslyn <c>DiagnosticDescriptor</c>）。
/// </summary>
/// <param name="id">诊断代码（如 <c>INI001</c>、<c>INI500</c>）。</param>
/// <param name="title">简短标题。</param>
/// <param name="messageFormat">消息格式（<c>{0}</c>/<c>{1}</c> 占位符由诊断参数填充）。</param>
/// <param name="category">类别（如 <c>Syntax</c>、<c>Analysis</c>）。</param>
/// <param name="defaultSeverity">默认严重级别。</param>
/// <param name="isEnabledByDefault">是否默认启用。</param>
/// <param name="description">详细说明。</param>
public sealed class DiagnosticDescriptor(
    string id,
    string title,
    string messageFormat,
    string category,
    DiagnosticSeverity defaultSeverity,
    bool isEnabledByDefault = true,
    string? description = null)
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
    /// 消息格式（占位符由 <see cref="Diagnostic.Create"/> 填充）。
    /// </summary>
    public string MessageFormat { get; } = messageFormat;

    /// <summary>
    /// 类别。
    /// </summary>
    public string Category { get; } = category;

    /// <summary>
    /// 默认严重级别。
    /// </summary>
    public DiagnosticSeverity DefaultSeverity { get; } = defaultSeverity;

    /// <summary>
    /// 是否默认启用。
    /// </summary>
    public bool IsEnabledByDefault { get; } = isEnabledByDefault;

    /// <summary>
    /// 详细说明。
    /// </summary>
    public string? Description { get; } = description;
}
