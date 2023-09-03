namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class TriggerObjectData(
    TriggerData Trigger,
    EventData Event,
    ActionData Action
);