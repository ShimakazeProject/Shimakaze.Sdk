namespace Shimakaze.Sdk.Map.Trigger;

/// <inheritdoc cref="Tag"/>
public sealed record class NullableTag(
    TagPersistence? Persistence,
    string? Name,
    string? TriggerId)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator Tag(NullableTag obj)
    {
        if (obj
            is { Persistence: null }
            or { Name: null }
            or { TriggerId: null }
        )
            throw new ArgumentException("This object's properties cannot be null");

        return new(obj.Persistence.Value, obj.Name, obj.TriggerId);
    }

    internal Tag Update(Tag old) => new(
        Persistence ?? old.Persistence,
        Name ?? old.Name,
        TriggerId ?? old.TriggerId
    );
};