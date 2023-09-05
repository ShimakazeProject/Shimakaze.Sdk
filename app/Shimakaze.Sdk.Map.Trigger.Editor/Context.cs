namespace Shimakaze.Sdk.Map.Trigger;

public sealed class Context
{
    public Dictionary<Guid, Session> Sessions { get; } = new();
}
