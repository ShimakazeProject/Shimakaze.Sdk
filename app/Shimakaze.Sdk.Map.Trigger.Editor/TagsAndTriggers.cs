namespace Shimakaze.Sdk.Map.Trigger;

public sealed record TagsAndTriggers(Dictionary<string, TagData> Tags, Dictionary<string, TriggerObjectData> Triggers);
