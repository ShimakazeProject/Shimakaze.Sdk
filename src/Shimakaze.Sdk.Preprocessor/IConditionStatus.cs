namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// 条件状态 为条件编译提供支持
/// </summary>
public interface IConditionStatus
{
    /// <summary>
    /// 条件
    /// </summary>
    string Condition { get; set; }

    /// <summary>
    /// 是否匹配
    /// </summary>
    bool IsMatched { get; set; }

    /// <summary>
    /// 标记
    /// </summary>
    string Tag { get; set; }
}