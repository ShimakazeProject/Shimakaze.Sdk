namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 标签循环类型
/// </summary>
public enum TagPersistence
{
    /// <summary>
    /// 不重复
    /// </summary>
    Volatile = 0,
    /// <summary>
    /// 半重复
    /// </summary>
    SemiPersistent = 1,
    /// <summary>
    /// 重复
    /// </summary>
    Persistent = 2,
}
