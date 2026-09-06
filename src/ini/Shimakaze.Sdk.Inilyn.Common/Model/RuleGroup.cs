namespace Shimakaze.Sdk.Inilyn.Model;

public sealed class RuleGroup
{
    public Dictionary<string, SectionDefinitionRule> Definitions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, RegistryRule> Registries { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, GlobalRule> Globals { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> EnumSections { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<DiscoverRule> Discover { get; } = [];
    public Dictionary<string, TypeDefinition> Types { get; } = new(StringComparer.OrdinalIgnoreCase);
}
