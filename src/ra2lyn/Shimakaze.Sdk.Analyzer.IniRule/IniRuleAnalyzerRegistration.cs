using Shimakaze.Sdk.Analyzer.Common;

namespace Shimakaze.Sdk.Analyzer.IniRule;

/// <summary>
/// 规则分析器的登记入口：宿主持有规则集时调用一次，即可让默认分析器集合包含规则校验。
/// </summary>
public static class IniRuleAnalyzerRegistration
{
    /// <summary>
    /// 把 <see cref="IniRuleAnalyzer"/> 登记为默认分析器；重复调用是同名替换，不会产生重复分析器。
    /// </summary>
    public static void Register()
        => AnalyzerRunner.RegisterDefault([new IniRuleAnalyzer()]);
}
