namespace Shimakaze.Sdk.Map.Trigger;

/// <inheritdoc cref="Trigger"/>
public sealed record class NullableTrigger(
    string? House,
    string? LinkedTrigger,
    string? Name,
    bool? Disable,
    bool? Easy,
    bool? Normal,
    bool? Hard,
    TriggerPersistence? Persistence
)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator Trigger(NullableTrigger obj)
    {
        if (obj
            is { House: null }
            or { LinkedTrigger: null }
            or { Name: null }
            or { Disable: null }
            or { Easy: null }
            or { Normal: null }
            or { Hard: null }
            or { Persistence: null }
        )
            throw new ArgumentException("This object's properties cannot be null");

        return new(
            obj.House,
            obj.LinkedTrigger,
            obj.Name,
            obj.Disable.Value,
            obj.Easy.Value,
            obj.Normal.Value,
            obj.Hard.Value,
            obj.Persistence.Value
        );
    }

    internal Trigger Update(Trigger old) => new(
        House ?? old.House,
        LinkedTrigger ?? old.LinkedTrigger,
        Name ?? old.Name,
        Disable ?? old.Disable,
        Easy ?? old.Easy,
        Normal ?? old.Normal,
        Hard ?? old.Hard,
        Persistence ?? old.Persistence
    );
}