namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class EventDataItem(
    int Event,
    int Parameter1,
    int Parameter2,
    string? Parameter3 = default
);
