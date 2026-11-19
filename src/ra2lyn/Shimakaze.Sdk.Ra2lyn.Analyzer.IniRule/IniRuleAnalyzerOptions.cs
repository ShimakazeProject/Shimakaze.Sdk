using Shimakaze.Sdk.Ra2lyn.Analyzer.Common;
using Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Types;

namespace Shimakaze.Sdk.Ra2lyn.Analyzer.IniRule;

/// <summary>
/// 规则分析器选项：在通用分析选项上携带校验模式与外部验证器。
/// </summary>
/// <remarks>
/// 规则集本身已经由基类的 <see cref="AnalyzerOptions.RuleSet"/> 承载，这里只补充与类型系统相关的输入。
/// </remarks>
public sealed class IniRuleAnalyzerOptions : AnalyzerOptions
{
    /// <summary>
    /// 值校验模式。
    /// </summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.Loose;

    /// <summary>
    /// 外部类型验证器集合。
    /// </summary>
    public IReadOnlyList<IExternalTypeValidator> ExternalValidators { get; init; } = [];
}
