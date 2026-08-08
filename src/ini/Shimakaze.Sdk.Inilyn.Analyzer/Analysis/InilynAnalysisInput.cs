namespace Shimakaze.Sdk.Inilyn.Analyzer.Analysis;

/// <summary>
/// 分析输入：一组编译产物的 INI 文本及其所属规则组。
/// </summary>
/// <param name="GroupName">规则组名（如 <c>Rule</c>/<c>Art</c>）。</param>
/// <param name="FileName">文件名（用于诊断定位）。</param>
/// <param name="Content">INI 内容。</param>
public sealed record class InilynAnalysisInput(string GroupName, string FileName, string Content);
