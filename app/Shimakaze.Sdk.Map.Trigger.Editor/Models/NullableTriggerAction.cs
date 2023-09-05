namespace Shimakaze.Sdk.Map.Trigger;

/// <inheritdoc cref="TriggerAction"/>
public sealed record class NullableTriggerAction(
    int? Count,
    IList<TriggerActionItem>? Items
)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator TriggerAction(NullableTriggerAction obj)
    {
        if (obj
            is { Count: null }
            or { Items: null }
        )
            throw new ArgumentException("This object's properties cannot be null");

        return new(obj.Count.Value, obj.Items);
    }

    internal TriggerAction Update(TriggerAction old) => new(
        Count ?? old.Count,
        Items ?? old.Items
    );
}