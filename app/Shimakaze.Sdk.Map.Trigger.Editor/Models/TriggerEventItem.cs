namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 事件
/// </summary>
/// <param name="Event"></param>
/// <param name="Parameter1"></param>
/// <param name="Parameter2"></param>
/// <param name="Parameter3"></param>
public sealed record class TriggerEventItem(
    int Event,
    int Parameter1,
    int Parameter2,
    string? Parameter3 = default
);
