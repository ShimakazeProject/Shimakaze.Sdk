namespace Shimakaze.Sdk.Map.Trigger;

/// <inheritdoc cref="TriggerEvent"/>
public sealed record class NullableTriggerEvent(
    int? Count,
    IList<TriggerEventItem>? Items
)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator TriggerEvent(NullableTriggerEvent obj)
    {
        if (obj
            is { Count: null }
            or { Items: null }
        )
            throw new ArgumentException("This object's properties cannot be null");

        return new(obj.Count.Value, obj.Items);
    }

    internal TriggerEvent Update(TriggerEvent old) => new(
        Count ?? old.Count,
        Items ?? old.Items
    );
}