namespace Shimakaze.Sdk.Inilyn.Analyzer.Analysis;

/// <summary>
/// 单个节的分析结果。
/// </summary>
/// <param name="groupName">组名。</param>
/// <param name="sectionName">节名。</param>
/// <param name="kind">分类。</param>
/// <param name="types">推断的类型集合（可为多个）。</param>
/// <param name="isReachable">是否从入口可达。</param>
public sealed class InilynSectionAnalysis(
    string groupName,
    string sectionName,
    InilynSectionKind kind,
    IReadOnlyList<string> types,
    bool isReachable)
{
    /// <summary>
    /// 组名。
    /// </summary>
    public string GroupName { get; } = groupName;

    /// <summary>
    /// 节名。
    /// </summary>
    public string SectionName { get; } = sectionName;

    /// <summary>
    /// 分类。
    /// </summary>
    public InilynSectionKind Kind { get; } = kind;

    /// <summary>
    /// 推断的类型集合（一个节可以同时是多个类型）。
    /// </summary>
    public IReadOnlyList<string> Types { get; } = types;

    /// <summary>
    /// 是否从入口可达（<see langword="false"/> 表示可被 TreeShaking 移除）。
    /// </summary>
    public bool IsReachable { get; } = isReachable;
}
