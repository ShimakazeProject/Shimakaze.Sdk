namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class Action(string Id, ActionData Data)
{
    public static Action Parse(KeyValuePair<string, string> trigger) => new(trigger.Key, ActionData.Parse(trigger.Value));
}
