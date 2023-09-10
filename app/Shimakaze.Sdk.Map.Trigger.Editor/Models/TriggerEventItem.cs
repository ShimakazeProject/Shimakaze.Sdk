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
){
    internal string ToIniValue() => Parameter3 is null
        ? $"{Event},{Parameter1},{Parameter2}"
        : $"{Event},{Parameter1},{Parameter2},{Parameter3}";
}
