namespace Shimakaze.Sdk.Inilyn.Model;

public sealed class RuleSet
{
    public Dictionary<string, RuleGroup> Groups { get; } = new(StringComparer.OrdinalIgnoreCase);
}
