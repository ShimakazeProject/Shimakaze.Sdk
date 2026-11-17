namespace Shimakaze.Sdk.Ra2lyn.Common.Source;

/// <summary>
/// 诊断严重级别。
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// 提示信息。
    /// </summary>
    Info,

    /// <summary>
    /// 警告，不阻止生成产物。
    /// </summary>
    Warning,

    /// <summary>
    /// 错误，表示产物不可用。
    /// </summary>
    Error,
}
