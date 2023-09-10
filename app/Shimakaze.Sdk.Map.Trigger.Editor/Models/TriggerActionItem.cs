namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 行为
/// </summary>
/// <param name="Action"></param>
/// <param name="Parameter1"></param>
/// <param name="Parameter2"></param>
/// <param name="Parameter3"></param>
/// <param name="Parameter4"></param>
/// <param name="Parameter5"></param>
/// <param name="Parameter6"></param>
/// <param name="Parameter7"></param>
public sealed record class TriggerActionItem(
    int Action,
    string Parameter1,
    string Parameter2,
    string Parameter3,
    string Parameter4,
    string Parameter5,
    string Parameter6,
    string Parameter7
)
{
    internal string ToIniValue() => $"{Action},{Parameter1},{Parameter2},{Parameter3},{Parameter4},{Parameter5},{Parameter6},{Parameter7}";
};
