namespace Shimakaze.Sdk.Inilyn.Analysis.RuleValidation;

/// <summary>
/// 验证模式，控制类型检查的严格程度。
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// 宽松模式：接受常见变体（如 <c>yes</c>/<c>No</c>、<c>50%</c> 后缀等）。
    /// </summary>
    Loose,

    /// <summary>
    /// 严格模式：仅接受标准格式（如 <c>true</c>/<c>false</c>、<c>0-100%</c>）。
    /// </summary>
    Strict,
}
