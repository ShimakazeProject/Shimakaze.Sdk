namespace Shimakaze.Sdk.Inilyn.Model;

public sealed class SectionDefinitionRule(string name)
{
    public string Name { get; } = name;
    public string? Base { get; internal set; }
    public Dictionary<string, KeyRule> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, KeyRule> GetEffectiveKeys(RuleGroup group)
    {
        var result = Base is not null && group.Definitions.TryGetValue(Base, out var parent)
            ? parent.GetEffectiveKeys(group)
            : new(StringComparer.OrdinalIgnoreCase);
        foreach ((string name, var key) in Keys) result[name] = key;
        return result;
    }
}
