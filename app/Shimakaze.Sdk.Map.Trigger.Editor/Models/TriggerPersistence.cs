namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 触发器的循环类型
/// </summary>
public enum TriggerPersistence
{
    /// <summary>
    /// 不重复
    /// </summary>
    Volatile = 0,
    /// <summary>
    /// 重复
    /// </summary>
    Persistent = 1,
}