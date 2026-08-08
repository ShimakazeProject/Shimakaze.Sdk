using System.Xml.Linq;

namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 规则集加载器：解析平台配置 XML，支持 <c>&lt;Include&gt;</c> 递归与多平台合并。
/// </summary>
public static class InilynRuleSetLoader
{
    private const string RootName = "InilynRules";

    /// <summary>
    /// 加载多个平台配置并合并。
    /// </summary>
    /// <param name="platformConfigPaths">平台配置文件路径数组。</param>
    /// <returns>合并后的规则集。</returns>
    public static InilynRuleSet Load(IEnumerable<string> platformConfigPaths)
    {
        ArgumentNullException.ThrowIfNull(platformConfigPaths);

        InilynRuleSet ruleSet = new();
        foreach (string path in platformConfigPaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
            LoadFile(ruleSet, Path.GetFullPath(path), visited);
        }

        return ruleSet;
    }

    private static void LoadFile(InilynRuleSet ruleSet, string path, HashSet<string> visited)
    {
        path = Path.GetFullPath(path);
        if (!visited.Add(path))
        {
            return; // 防循环包含
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"规则文件不存在：{path}", path);
        }

        XDocument doc = XDocument.Load(path);
        var root = doc.Root;
        if (root is null || root.Name.LocalName != RootName)
        {
            throw new FormatException($"非法的规则文件根元素 '{root?.Name.LocalName}'（应为 {RootName}）：{path}");
        }

        string dir = Path.GetDirectoryName(path)!;

        foreach (var include in root.Elements("Include"))
        {
            string? includePath = (string?)include.Attribute("Path");
            if (string.IsNullOrWhiteSpace(includePath))
            {
                continue;
            }

            LoadFile(ruleSet, Path.Combine(dir, includePath), visited);
        }

        foreach (var typeElement in root.Elements("Type"))
        {
            LoadType(ruleSet, typeElement);
        }

        foreach (var enumElement in root.Elements("Enum"))
        {
            LoadEnum(ruleSet, enumElement);
        }

        foreach (var groupElement in root.Elements("Group"))
        {
            LoadGroup(ruleSet, groupElement);
        }
    }

    private static void LoadType(InilynRuleSet ruleSet, XElement element)
    {
        string? name = (string?)element.Attribute("Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        string? external = (string?)element.Attribute("External");
        bool isExternal = string.Equals(external, "true", StringComparison.OrdinalIgnoreCase);

        List<InilynTupleField> fields = [];
        foreach (var field in element.Elements("Field"))
        {
            string? fieldName = (string?)field.Attribute("Name");
            string? fieldType = (string?)field.Attribute("Type");
            if (fieldName is null || fieldType is null)
            {
                continue;
            }

            fields.Add(new InilynTupleField(fieldName, fieldType));
        }

        InilynValueType type;
        if (isExternal)
        {
            type = new InilynValueType(name, InilynValueTypeKind.External, externalKind: name);
        }
        else if (fields.Count > 0)
        {
            string separator = (string?)element.Attribute("Separator") ?? ",";
            type = new InilynValueType(name, InilynValueTypeKind.Tuple, separator, fields);
        }
        else
        {
            type = new InilynValueType(name, InilynValueTypeKind.Builtin);
        }

        ruleSet.AddType(type);
    }

    private static void LoadEnum(InilynRuleSet ruleSet, XElement element)
    {
        string? name = (string?)element.Attribute("Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        InilynEnum en = new(name);
        foreach (var value in element.Elements("Value"))
        {
            string? v = value.Value.Trim();
            if (!string.IsNullOrEmpty(v))
            {
                en.AddRange([v]);
            }
        }

        ruleSet.AddEnum(en);
    }

    private static void LoadGroup(InilynRuleSet ruleSet, XElement element)
    {
        string? name = (string?)element.Attribute("Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var group = ruleSet.GetOrAddGroup(name);

        foreach (var def in element.Elements("SectionDefinition"))
        {
            LoadSectionDefinition(group, def);
        }

        foreach (var registry in element.Elements("Registry"))
        {
            string? section = (string?)registry.Attribute("Section");
            string? elem = (string?)registry.Attribute("Element");
            if (section is null || elem is null)
            {
                continue;
            }

            group.AddRegistry(new InilynRegistryDeclaration(section, elem));
        }

        foreach (var es in element.Elements("EnumSection"))
        {
            string? section = (string?)es.Attribute("Section");
            if (section is null)
            {
                continue;
            }

            group.AddEnumSection(new InilynEnumSectionDeclaration(
                section,
                (string?)es.Attribute("Enum"),
                (string?)es.Attribute("ValueType") ?? "string",
                (string?)es.Attribute("List")));
        }

        foreach (var g in element.Elements("Global"))
        {
            string? section = (string?)g.Attribute("Section");
            if (section is null)
            {
                continue;
            }

            group.AddGlobal(new InilynGlobalDeclaration(section, (string?)g.Attribute("Type")));
        }

        foreach (var d in element.Elements("Discover"))
        {
            foreach (var rule in d.Elements("Rule"))
            {
                string? target = (string?)rule.Attribute("Target");
                if (string.IsNullOrWhiteSpace(target))
                {
                    continue;
                }

                group.AddDiscovery(new InilynDiscoveryRule(
                    target,
                    (string?)rule.Attribute("From"),
                    (string?)rule.Attribute("ResolveKey"),
                    (string?)rule.Attribute("Fallback")));
            }
        }
    }

    private static void LoadSectionDefinition(InilynRuleGroup group, XElement element)
    {
        string? name = (string?)element.Attribute("Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        InilynSectionDefinition def = new(name);
        def.SetBase((string?)element.Attribute("Base"));

        foreach (var key in element.Elements("Key"))
        {
            string? keyName = (string?)key.Attribute("Name");
            if (keyName is null)
            {
                continue;
            }

            def.AddKey(new InilynKeyDeclaration(keyName, (string?)key.Attribute("Type") ?? "string", (string?)key.Attribute("List")));
        }

        group.AddDefinition(def);
    }
}
