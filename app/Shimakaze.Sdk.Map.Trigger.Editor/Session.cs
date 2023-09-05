using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.Map.Trigger;

public sealed class Session
{
    public required string Path { get; init; }
    public required IniDocument Ini { get; init; }
    public required Dictionary<string, Tag> Tags { get; init; }
    public required Dictionary<string, Trigger> Triggers { get; init; }
    public required Dictionary<string, TriggerEvent> Events { get; init; }
    public required Dictionary<string, TriggerAction> Actions { get; init; }
}
