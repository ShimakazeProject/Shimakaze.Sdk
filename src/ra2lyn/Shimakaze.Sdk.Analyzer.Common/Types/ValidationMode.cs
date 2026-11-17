namespace Shimakaze.Sdk.Analyzer.Common.Types;

/// <summary>
/// 值校验的严格程度。
/// </summary>
public enum ValidationMode
{
    /// <summary>
    /// 宽松模式：接受常见变体，例如以 <c>T</c>/<c>Y</c>/<c>1</c> 开头的布尔值、任意以数字开头的百分比。
    /// </summary>
    Loose,

    /// <summary>
    /// 严格模式：只接受规范写法。
    /// </summary>
    Strict,
}
