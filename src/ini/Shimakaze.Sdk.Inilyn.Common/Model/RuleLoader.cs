using System.Xml;

namespace Shimakaze.Sdk.Inilyn.Model;

public static class RuleLoader
{
    public static async Task<RuleSet> LoadAsync(string path)
    {
        RuleSet result = new();
        await LoadFileAsync(result, Path.GetFullPath(path), new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        return result;
    }

    private static async Task LoadFileAsync(RuleSet result, string path, HashSet<string> loaded)
    {
        if (!loaded.Add(path)) return;

        string directory = Path.GetDirectoryName(path)!;

        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true,
        });

        RuleGroup? currentGroup = null;

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;

            switch (reader.Name)
            {
                case "Include":
                    string? includePath = reader.GetAttribute("Path");
                    if (!string.IsNullOrWhiteSpace(includePath))
                    {
                        await LoadFileAsync(result, Path.Combine(directory, includePath), loaded);
                    }
                    break;

                case "Group":
                    string? groupName = reader.GetAttribute("Name");
                    if (!string.IsNullOrWhiteSpace(groupName))
                    {
                        currentGroup = result.Groups.GetValueOrDefault(groupName) ?? new RuleGroup();
                        result.Groups[groupName] = currentGroup;
                    }
                    break;

                case "SectionDefinition":
                    if (currentGroup is not null)
                    {
                        string? defName = reader.GetAttribute("Name");
                        if (!string.IsNullOrWhiteSpace(defName))
                        {
                            var definition = currentGroup.Definitions.GetValueOrDefault(defName) ?? new SectionDefinitionRule(defName);
                            currentGroup.Definitions[defName] = definition;
                            definition.Base = reader.GetAttribute("Base") ?? definition.Base;
                        }
                    }
                    break;

                case "Key":
                    if (currentGroup is not null && reader.Depth >= 2)
                    {
                        string? keyName = reader.GetAttribute("Name");
                        if (keyName is not null)
                        {
                            var parent = GetParentDefinition(currentGroup);
                            parent?.Keys[keyName] = new KeyRule(reader.GetAttribute("Type") ?? "string", reader.GetAttribute("List"));
                        }
                    }
                    break;

                case "Registry":
                    if (currentGroup is not null)
                    {
                        string? section = reader.GetAttribute("Section");
                        string? element = reader.GetAttribute("Element");
                        if (section is not null && element is not null)
                        {
                            currentGroup.Registries[section] = new RegistryRule(element);
                        }
                    }
                    break;

                case "Global":
                    if (currentGroup is not null)
                    {
                        string? section = reader.GetAttribute("Section");
                        if (section is not null)
                        {
                            currentGroup.Globals[section] = new GlobalRule(section, reader.GetAttribute("Type") ?? section);
                        }
                    }
                    break;

                case "EnumSection":
                    if (currentGroup is not null)
                    {
                        string? section = reader.GetAttribute("Section");
                        if (section is not null)
                        {
                            currentGroup.EnumSections[section] = section;
                        }
                    }
                    break;

                case "Discover":
                    if (currentGroup is not null)
                    {
                        await LoadDiscoverRulesAsync(currentGroup.Discover, reader);
                    }
                    break;

                case "Type":
                    if (currentGroup is not null)
                    {
                        string? typeName = reader.GetAttribute("Name");
                        if (!string.IsNullOrWhiteSpace(typeName))
                        {
                            var typeDef = new TypeDefinition
                            {
                                Name = typeName,
                                Separator = reader.GetAttribute("Separator"),
                            };
                            await LoadTypeFieldsAsync(typeDef, reader);
                            currentGroup.Types[typeName] = typeDef;
                        }
                    }
                    break;
            }
        }
    }

    private static async Task LoadDiscoverRulesAsync(List<DiscoverRule> discover, XmlReader reader)
    {
        if (reader.IsEmptyElement) return;

        int depth = reader.Depth;
        while (await reader.ReadAsync() && reader.Depth > depth)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "Rule")
            {
                discover.Add(new DiscoverRule(
                    reader.GetAttribute("From"),
                    reader.GetAttribute("ResolveKey"),
                    reader.GetAttribute("Target")!,
                    reader.GetAttribute("Fallback"),
                    reader.GetAttribute("Min"),
                    reader.GetAttribute("Max")));
            }
        }
    }

    private static async Task LoadTypeFieldsAsync(TypeDefinition typeDef, XmlReader reader)
    {
        if (reader.IsEmptyElement) return;

        int depth = reader.Depth;
        while (await reader.ReadAsync() && reader.Depth > depth)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "Field")
            {
                string? fieldName = reader.GetAttribute("Name");
                string? fieldType = reader.GetAttribute("Type");
                if (fieldName is not null && fieldType is not null)
                {
                    typeDef.Fields.Add(new TypeField(fieldName, fieldType));
                }
            }
        }
    }

    private static SectionDefinitionRule? GetParentDefinition(RuleGroup group)
    {
        SectionDefinitionRule? last = null;
        foreach (var def in group.Definitions.Values)
        {
            last = def;
        }
        return last;
    }
}
