using System.Xml;
using System.Xml.Linq;

namespace Shimakaze.Sdk.Analyzer.Common.Rules;

/// <summary>
/// 规则 XML 读取器，兼容现有 <c>Ra2lynRules.xsd</c> 的元素结构。
/// </summary>
public static class RuleSetReader
{
    /// <summary>
    /// 同步读取规则集；<c>Include</c> 支持相对路径、递归与循环去重。
    /// </summary>
    /// <param name="path">规则入口 XML 文件路径。</param>
    public static RuleSet Read(string path)
        => ReadFilesAsync([Path.GetFullPath(path)]).GetAwaiter().GetResult();

    /// <summary>
    /// 依次读取多个规则入口，结果合并到同一个规则集。
    /// </summary>
    /// <param name="paths">规则入口 XML 文件路径列表。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static async Task<RuleSet> ReadFilesAsync(IReadOnlyList<string> paths, CancellationToken cancellationToken = default)
    {
        RuleFileReader reader = new();
        foreach (string path in paths)
            await reader.ReadFileAsync(Path.GetFullPath(path), cancellationToken);

        return reader.Result;
    }

    /// <summary>
    /// 顶层 <c>Type</c>/<c>Enum</c> 不属于任何规则组；登记到该组，供各规则组合并使用。
    /// </summary>
    internal const string DefaultGroupName = "default";
}

/// <summary>
/// 规则文件读取状态：跟踪已加载文件并累积读取结果。
/// </summary>
file sealed class RuleFileReader
{
    private readonly HashSet<string> _loaded = new(StringComparer.Ordinal);

    public RuleSet Result { get; } = new();

    public async Task ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        if (!_loaded.Add(path))
            return;

        string directory = Path.GetDirectoryName(path) ?? string.Empty;
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

        XDocument document;
        using (XmlReader xmlReader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            CloseInput = false,
            DtdProcessing = DtdProcessing.Ignore,
        }))
        {
            document = await XDocument.LoadAsync(xmlReader, LoadOptions.None, cancellationToken);
        }

        await ReadDocumentAsync(document, directory, cancellationToken);
    }

    private async Task ReadDocumentAsync(XDocument document, string directory, CancellationToken cancellationToken)
    {
        foreach (XElement element in document.Root?.Elements() ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (element.Name.LocalName)
            {
                case "Include":
                    if (element.Attribute("Path")?.Value is { Length: > 0 } includePath)
                        await ReadFileAsync(Path.Combine(directory, includePath), cancellationToken);
                    break;
                case "Type":
                    ReadType(element);
                    break;
                case "Enum":
                    ReadEnum(element);
                    break;
                case "Group":
                    ReadGroup(element);
                    break;
            }
        }
    }

    private void ReadGroup(XElement element)
    {
        if (element.Attribute("Name")?.Value is not { Length: > 0 } groupName)
            return;

        RuleGroup group = GetOrCreateGroup(groupName);
        foreach (XElement child in element.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "SectionDefinition":
                    ReadSectionDefinition(group, child);
                    break;
                case "Registry":
                    ReadRegistry(group, child);
                    break;
                case "Global":
                    ReadGlobal(group, child);
                    break;
                case "EnumSection":
                    ReadEnumSection(group, child);
                    break;
                case "Discover":
                    ReadDiscover(group, child);
                    break;
            }
        }
    }

    private static void ReadSectionDefinition(RuleGroup group, XElement element)
    {
        if (element.Attribute("Name")?.Value is not { Length: > 0 } definitionName)
            return;

        SectionDefinitionRule definition = group.Definitions.TryGetValue(definitionName, out var existing)
            ? existing
            : new SectionDefinitionRule(definitionName);

        if (element.Attribute("Base")?.Value is { } baseName)
            definition.Base = baseName;

        group.Definitions[definitionName] = definition;

        // Key 只在当前 SectionDefinition 内生效：直接取该元素的子节点，不依赖“最后定义”。
        foreach (XElement keyElement in element.Elements("Key"))
        {
            if (keyElement.Attribute("Name")?.Value is not { Length: > 0 } keyName)
                continue;

            definition.Keys[keyName] = new KeyRule(
                keyElement.Attribute("Type")?.Value ?? "string",
                keyElement.Attribute("List")?.Value);
        }
    }

    private static void ReadRegistry(RuleGroup group, XElement element)
    {
        string? section = element.Attribute("Section")?.Value;
        string? registeredElement = element.Attribute("Element")?.Value;
        if (section is not null && registeredElement is not null)
            group.Registries[section] = new RegistryRule(registeredElement);
    }

    private static void ReadGlobal(RuleGroup group, XElement element)
    {
        if (element.Attribute("Section")?.Value is not { Length: > 0 } section)
            return;

        group.Globals[section] = new GlobalRule(section, element.Attribute("Type")?.Value);
    }

    private static void ReadEnumSection(RuleGroup group, XElement element)
    {
        if (element.Attribute("Section")?.Value is not { Length: > 0 } section)
            return;

        group.EnumSections[section] = new EnumSectionRule(
            section,
            element.Attribute("Enum")?.Value,
            element.Attribute("ValueType")?.Value,
            element.Attribute("List")?.Value);
    }

    private static void ReadDiscover(RuleGroup group, XElement element)
    {
        foreach (XElement ruleElement in element.Elements("Rule"))
        {
            if (ruleElement.Attribute("Target")?.Value is not { Length: > 0 } target)
                continue;

            group.Discover.Add(new DiscoverRule(
                ruleElement.Attribute("From")?.Value,
                ruleElement.Attribute("ResolveKey")?.Value,
                target,
                ruleElement.Attribute("Fallback")?.Value,
                ruleElement.Attribute("Min")?.Value,
                ruleElement.Attribute("Max")?.Value));
        }
    }

    private void ReadType(XElement element)
    {
        if (element.Attribute("Name")?.Value is not { Length: > 0 } typeName)
            return;

        TypeDefinition definition = new()
        {
            Name = typeName,
            Separator = element.Attribute("Separator")?.Value,
        };

        foreach (XElement fieldElement in element.Elements("Field"))
        {
            string? fieldName = fieldElement.Attribute("Name")?.Value;
            string? fieldType = fieldElement.Attribute("Type")?.Value;
            if (fieldName is not null && fieldType is not null)
                definition.Fields.Add(new TypeField(fieldName, fieldType));
        }

        GetOrCreateGroup(RuleSetReader.DefaultGroupName).Types[typeName] = definition;
    }

    private void ReadEnum(XElement element)
    {
        if (element.Attribute("Name")?.Value is not { Length: > 0 } enumName)
            return;

        EnumDefinition definition = new() { Name = enumName };
        foreach (XElement valueElement in element.Elements("Value"))
            definition.Values.Add(valueElement.Value.Trim());

        GetOrCreateGroup(RuleSetReader.DefaultGroupName).Enums[enumName] = definition;
    }

    private RuleGroup GetOrCreateGroup(string name)
    {
        if (Result.Groups.TryGetValue(name, out var existing))
            return existing;

        RuleGroup created = new();
        Result.Groups[name] = created;
        return created;
    }
}