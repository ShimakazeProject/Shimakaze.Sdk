using Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;
using Shimakaze.Sdk.Inilyn.Syntax;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;
using Shimakaze.Sdk.Inilyn.Syntax.Parsing;
using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn.Analyzer.Analysis;

/// <summary>
/// Inilyn 分析器：分类节、校验键值、构建跨文件引用图并计算可达性（TreeShaking）。
/// </summary>
public static class InilynAnalyzer
{
    /// <summary>
    /// 对编译产物执行分析。
    /// </summary>
    /// <param name="ruleSet">规则集。</param>
    /// <param name="inputs">编译产物（每个输入 = 一个规则组的一份 INI 内容）。</param>
    /// <param name="externalAssets">可选的外部资源清单（种类 → 合法值集合）。</param>
    /// <returns>分析结果。</returns>
    public static InilynAnalysis Analyze(
        InilynRuleSet ruleSet,
        IEnumerable<InilynAnalysisInput> inputs,
        IReadOnlyDictionary<string, IReadOnlySet<string>>? externalAssets = null)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        ArgumentNullException.ThrowIfNull(inputs);
        return new AnalyzerCore(ruleSet, inputs, externalAssets).Run();
    }

    /// <summary>
    /// 便捷重载：单组编译结果的便捷分析入口。
    /// </summary>
    /// <param name="ruleSet">规则集。</param>
    /// <param name="groupName">规则组名。</param>
    /// <param name="compilationResult">编译结果。</param>
    /// <param name="externalAssets">外部资源清单。</param>
    /// <returns>分析结果。</returns>
    public static InilynAnalysis Analyze(
        InilynRuleSet ruleSet,
        string groupName,
        Compilation.InilynCompilationResult compilationResult,
        IReadOnlyDictionary<string, IReadOnlySet<string>>? externalAssets = null)
    {
        ArgumentNullException.ThrowIfNull(compilationResult);
        List<InilynAnalysisInput> inputs = [];
        foreach ((string fileName, string content) in compilationResult.OutputFiles)
        {
            inputs.Add(new InilynAnalysisInput(groupName, fileName, content));
        }

        return Analyze(ruleSet, inputs, externalAssets);
    }
}

/// <summary>
/// 分析器内部实现（持有分析期间的全部状态）。
/// </summary>
internal sealed class AnalyzerCore
{
    private static readonly IEqualityComparer<(string Group, string Name)> SecKeyComparer =
        new SecKeyEqualityComparer();

    private readonly InilynRuleSet _ruleSet;
    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>>? _externalAssets;
    private readonly Dictionary<string, Dictionary<string, ParsedIniSection>> _sections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(string, string), string> _fileNames = new(SecKeyComparer);
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly Dictionary<(string, string), InilynSectionKind> _kinds = new(SecKeyComparer);
    private readonly Dictionary<(string, string), HashSet<string>> _types = new(SecKeyComparer);
    private readonly Dictionary<(string, string), HashSet<(string, string)>> _edges = new(SecKeyComparer);

    public AnalyzerCore(
        InilynRuleSet ruleSet,
        IEnumerable<InilynAnalysisInput> inputs,
        IReadOnlyDictionary<string, IReadOnlySet<string>>? externalAssets)
    {
        _ruleSet = ruleSet;
        _externalAssets = externalAssets;

        foreach (var input in inputs)
        {
            if (!_sections.TryGetValue(input.GroupName, out var map))
            {
                map = new Dictionary<string, ParsedIniSection>(StringComparer.OrdinalIgnoreCase);
                _sections[input.GroupName] = map;
            }

            foreach (var s in ParseSections(input.Content, input.FileName))
            {
                map[s.Name] = s;
                _fileNames[(input.GroupName, s.Name)] = input.FileName;
            }
        }
    }

    public InilynAnalysis Run()
    {
        ClassifyRegistries();
        ClassifyEnumSections();
        ClassifyGlobals();
        ApplyDiscoveries();
        ScanReferences();
        ValidateEnumSections();
        ReportMultiType();
        var reachable = ComputeReachability();
        return BuildResult(reachable);
    }

    private InilynRuleGroup? Group(string name)
    {
        return _ruleSet.TryGetGroup(name, out var g) ? g : null;
    }

    private ParsedIniSection? GetSection(string group, string name)
    {
        return _sections.TryGetValue(group, out var map)
            ? map.GetValueOrDefault(name)
            : null;
    }

    private string? FileNameOf(string group, string name)
    {
        return _fileNames.GetValueOrDefault((group, name));
    }

    private void SetKind(string group, string name, InilynSectionKind kind)
    {
        _kinds[(group, name)] = kind;
    }

    private void AddType(string group, string name, string typeName)
    {
        (string, string) key = (group, name);
        if (!_types.TryGetValue(key, out var set))
        {
            set = [];
            _types[key] = set;
        }

        set.Add(typeName);
    }

    private void AddEdge((string, string) from, (string, string) to)
    {
        if (!_edges.TryGetValue(from, out var set))
        {
            set = [];
            _edges[from] = set;
        }

        set.Add(to);
    }

    private void ClassifyRegistries()
    {
        foreach (var group in _ruleSet.Groups.Values)
        {
            foreach (var reg in group.Registries.Values)
            {
                var regSection = GetSection(group.Name, reg.Section);
                if (regSection is null)
                {
                    continue;
                }

                SetKind(group.Name, reg.Section, InilynSectionKind.Registry);

                (string targetGroup, string elementType) = SplitDotted(reg.Element, group.Name);
                foreach (var kv in regSection.Keys)
                {
                    string memberName = kv.Value.Trim();
                    if (memberName.Length == 0)
                    {
                        continue;
                    }

                    if (GetSection(targetGroup, memberName) is null)
                    {
                        var (ml, mc, mel, mec) = regSection.KeyPositions.GetValueOrDefault(kv.Key);
                        _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.RegistryMemberMissingSection,
                            ml, mc, mel, mec, FileNameOf(group.Name, reg.Section), reg.Section, memberName, targetGroup));
                        continue;
                    }

                    AddType(targetGroup, memberName, elementType);
                }
            }
        }
    }

    private void ClassifyEnumSections()
    {
        foreach (var group in _ruleSet.Groups.Values)
        {
            foreach (var es in group.EnumSections.Values)
            {
                if (GetSection(group.Name, es.Section) is not null)
                {
                    SetKind(group.Name, es.Section, InilynSectionKind.EnumSection);
                }
            }
        }
    }

    private void ClassifyGlobals()
    {
        foreach (var group in _ruleSet.Groups.Values)
        {
            foreach (var g in group.Globals.Values)
            {
                if (GetSection(group.Name, g.Section) is null)
                {
                    continue;
                }

                SetKind(group.Name, g.Section, InilynSectionKind.Global);
                string type = g.Type ?? g.Section;
                if (group.GetDefinition(type) is not null)
                {
                    AddType(group.Name, g.Section, type);
                }
            }
        }
    }

    private void ApplyDiscoveries()
    {
        foreach (var group in _ruleSet.Groups.Values)
        {
            foreach (var disc in group.Discoveries)
            {
                if (string.IsNullOrWhiteSpace(disc.From) || string.IsNullOrWhiteSpace(disc.ResolveKey))
                {
                    continue;
                }

                (string targetGroup, string targetType) = SplitDotted(disc.Target, group.Name);
                foreach (((string Group, string Name) key, var set) in _types.ToArray())
                {
                    if (!string.Equals(key.Group, group.Name, StringComparison.OrdinalIgnoreCase)
                        || !set.Any(t => MatchesType(group, t, disc.From)))
                    {
                        continue;
                    }

                    var source = GetSection(group.Name, key.Name);
                    if (source is null)
                    {
                        continue;
                    }

                    string? targetName = source.Keys.TryGetValue(disc.ResolveKey, out string? resolvedValue)
                        ? resolvedValue.Trim()
                        : null;

                    targetName ??= string.Equals(disc.Fallback, "self", StringComparison.OrdinalIgnoreCase)
                        ? source.Name
                        : null;

                    if (string.IsNullOrWhiteSpace(targetName))
                    {
                        continue;
                    }

                    if (GetSection(targetGroup, targetName) is null)
                    {
                        _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.DiscoverTargetMissing,
                            source.Line, source.Column, source.EndLine, source.EndColumn, FileNameOf(group.Name, source.Name), group.Name, source.Name, targetName, targetGroup));
                        continue;
                    }

                    AddType(targetGroup, targetName, targetType);
                    AddEdge((group.Name, source.Name), (targetGroup, targetName));
                }
            }
        }
    }

    private void ScanReferences()
    {
        Queue<(string, string)> queue = new();
        foreach (var key in _types.Keys)
        {
            queue.Enqueue(key);
        }

        HashSet<(string, string)> processed = new(SecKeyComparer);
        while (queue.Count > 0)
        {
            (string groupName, string sectionName) = queue.Dequeue();
            if (!processed.Add((groupName, sectionName)))
            {
                continue;
            }

            var section = GetSection(groupName, sectionName);
            var group = Group(groupName);
            if (section is null || group is null)
            {
                continue;
            }

            if (_kinds.GetValueOrDefault((groupName, sectionName)) is InilynSectionKind.EnumSection
                or InilynSectionKind.Registry)
            {
                continue;
            }

            var effective = GetEffectiveKeys(groupName, sectionName, group);
            if (effective.Count == 0)
            {
                continue;
            }

            foreach (var kv in section.Keys)
            {
                // 数字键是动态列表项（AI 任务部队等），按约定跳过
                if (kv.Key.Length > 0 && kv.Key.All(static c => c is >= '0' and <= '9'))
                {
                    continue;
                }

                if (!effective.TryGetValue(kv.Key, out var decl))
                {
                    var (kl, kc, kel, kec) = section.KeyPositions.GetValueOrDefault(kv.Key);
                    _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.UnknownKey,
                        kl, kc, kel, kec, FileNameOf(groupName, sectionName), sectionName, kv.Key));
                    continue;
                }

                ValidateKey(groupName, sectionName, section, decl, kv.Value, queue);
            }
        }
    }

    private Dictionary<string, InilynKeyDeclaration> GetEffectiveKeys(
        string groupName,
        string sectionName,
        InilynRuleGroup group)
    {
        Dictionary<string, InilynKeyDeclaration> effective = new(StringComparer.OrdinalIgnoreCase);
        if (!_types.TryGetValue((groupName, sectionName), out var sectionTypeSet))
        {
            return effective;
        }

        foreach (string t in sectionTypeSet)
        {
            var def = group.GetDefinition(t);
            if (def is null)
            {
                continue;
            }

            foreach ((string k, var v) in def.GetEffectiveKeys(group))
            {
                effective[k] = v;
            }
        }

        return effective;
    }

    private void ValidateKey(
        string groupName,
        string sectionName,
        ParsedIniSection section,
        InilynKeyDeclaration decl,
        string rawValue,
        Queue<(string, string)> queue)
    {
        string value = rawValue.Trim();
        var members = InilynValueRefResolver.ResolveAll(_ruleSet, groupName, decl.Type);
        string[] elements = decl.List is not null
            ? value.Split(decl.List, StringSplitOptions.TrimEntries)
            : [value];

        foreach (string element in elements)
        {
            if (InilynValueValidator.IsExemptReferenceValue(element))
            {
                continue;
            }

            bool matched = false;
            foreach (var member in members)
            {
                matched = member.Kind switch
                {
                    InilynValueRefKind.RegistryRef => TryValidateRegistryReference(
                        groupName, element, member),
                    InilynValueRefKind.SectionRef => TryValidateSectionReference(
                        groupName, sectionName, element, member, queue),
                    _ => InilynValueValidator.IsValidScalar(_ruleSet, member, element, _externalAssets),
                };

                if (matched)
                {
                    break;
                }
            }

            if (matched)
            {
                continue;
            }

            var (kl, kc, kel, kec) = section.KeyPositions.GetValueOrDefault(decl.Name);
            var descriptor = members.Any(m => m.Kind == InilynValueRefKind.Enum)
                ? InilynDiagnostics.EnumValueInvalid
                : members.Any(m => m.IsReference)
                    ? InilynDiagnostics.ReferenceMissing
                    : InilynDiagnostics.ValueTypeMismatch;
            _diagnostics.Add(Diagnostic.Create(descriptor,
                kl, kc, kel, kec, FileNameOf(groupName, sectionName), sectionName, decl.Name, element, decl.Type));
        }
    }

    private bool TryValidateSectionReference(
        string groupName,
        string sectionName,
        string name,
        InilynResolvedValueType resolved,
        Queue<(string, string)> queue)
    {
        string targetGroup = resolved.TargetGroup ?? groupName;

        // 优先整值匹配；未命中且含分隔符时拆分兜底
        string[] candidates = GetSection(targetGroup, name) is not null
            ? [name]
            : GetReferenceTargets(name, null);

        bool allFound = true;
        foreach (string candidate in candidates)
        {
            if (InilynValueValidator.IsExemptReferenceValue(candidate))
            {
                continue;
            }

            var target = GetSection(targetGroup, candidate);
            if (target is null)
            {
                allFound = false;
                break;
            }

            AddType(targetGroup, candidate, resolved.TargetName!);
            AddEdge((groupName, sectionName), (targetGroup, candidate));
            queue.Enqueue((targetGroup, candidate));
        }

        return allFound;
    }

    private bool TryValidateRegistryReference(
        string groupName,
        string memberName,
        InilynResolvedValueType resolved)
    {
        string targetGroup = resolved.TargetGroup ?? groupName;
        var targetGroupObj = Group(targetGroup);
        var reg = targetGroupObj?.GetRegistry(resolved.TargetName!);
        if (reg is null)
        {
            return false;
        }

        var regSection = GetSection(targetGroup, reg.Section);
        return regSection is not null && regSection.Keys.Values.Any(v =>
            string.Equals(v.Trim(), memberName, StringComparison.OrdinalIgnoreCase));
    }

    private void ValidateEnumSections()
    {
        foreach (var group in _ruleSet.Groups.Values)
        {
            foreach (var es in group.EnumSections.Values)
            {
                var section = GetSection(group.Name, es.Section);
                if (section is null)
                {
                    continue;
                }

                var en = es.Enum is not null ? _ruleSet.GetEnum(es.Enum) : null;
                foreach (var kv in section.Keys)
                {
                    if (en is not null && !en.Values.Contains(kv.Key))
                    {
                        var (kl, kc, kel, kec) = section.KeyPositions.GetValueOrDefault(kv.Key);
                        _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.EnumKeyInvalid,
                            kl, kc, kel, kec, FileNameOf(group.Name, section.Name), es.Section, kv.Key, en.Name));
                    }

                    if (es.ValueType == "string")
                    {
                        continue;
                    }

                    var vt = InilynValueRefResolver.Resolve(_ruleSet, group.Name, es.ValueType);
                    string value = es.List is not null ? kv.Value.Split(es.List, StringSplitOptions.TrimEntries)[0] : kv.Value;
                    if (InilynValueValidator.IsValidScalar(_ruleSet, vt, value, _externalAssets))
                    {
                        continue;
                    }

                    var (l, c, el, ec) = section.KeyPositions.GetValueOrDefault(kv.Key);
                    _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.ValueTypeMismatch,
                        l, c, el, ec, FileNameOf(group.Name, section.Name), es.Section, kv.Key, kv.Value, es.ValueType));
                }
            }
        }
    }

    private void ReportMultiType()
    {
        foreach (((string Group, string Name) key, var set) in _types)
        {
            if (set.Count <= 1)
            {
                continue;
            }

            var s = GetSection(key.Group, key.Name);
            _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.MultiType,
                s?.Line ?? 0, s?.Column ?? 0, s?.EndLine ?? 0, s?.EndColumn ?? 0, FileNameOf(key.Group, key.Name), key.Name, string.Join(", ", set.OrderBy(x => x))));
        }
    }

    private HashSet<(string, string)> ComputeReachability()
    {
        HashSet<(string, string)> reachable = new(SecKeyComparer);
        Queue<(string, string)> frontier = new();
        void Mark((string, string) sec)
        {
            if (reachable.Add(sec))
            {
                frontier.Enqueue(sec);
            }
        }

        foreach (var group in _ruleSet.Groups.Values)
        {
            foreach (var g in group.Globals.Values)
            {
                if (GetSection(group.Name, g.Section) is not null)
                {
                    Mark((group.Name, g.Section));
                }
            }

            foreach (var es in group.EnumSections.Values)
            {
                if (GetSection(group.Name, es.Section) is not null)
                {
                    Mark((group.Name, es.Section));
                }
            }

            foreach (var reg in group.Registries.Values)
            {
                var regSection = GetSection(group.Name, reg.Section);
                if (regSection is null)
                {
                    continue;
                }

                Mark((group.Name, reg.Section));
                (string targetGroup, _) = SplitDotted(reg.Element, group.Name);
                foreach (string member in regSection.Keys.Values)
                {
                    string m = member.Trim();
                    if (m.Length > 0 && GetSection(targetGroup, m) is not null)
                    {
                        Mark((targetGroup, m));
                    }
                }
            }
        }

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (!_edges.TryGetValue(current, out var targets))
            {
                continue;
            }

            foreach (var t in targets)
            {
                Mark(t);
            }
        }

        return reachable;
    }

    private InilynAnalysis BuildResult(HashSet<(string, string)> reachable)
    {
        List<InilynSectionAnalysis> result = [];
        foreach (var groupEntry in _sections)
        {
            foreach (var s in groupEntry.Value.Values)
            {
                (string, string) key = (groupEntry.Key, s.Name);
                var kind = _kinds.GetValueOrDefault(key, InilynSectionKind.Unknown);
                if (kind == InilynSectionKind.Unknown && _types.ContainsKey(key))
                {
                    kind = InilynSectionKind.Entity;
                }

                if (kind == InilynSectionKind.Unknown)
                {
                    _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.UnknownSection,
                        s.Line, s.Column, s.EndLine, s.EndColumn, FileNameOf(groupEntry.Key, s.Name), s.Name));
                }

                bool isReachable = reachable.Contains(key);
                if (!isReachable && _kinds.ContainsKey(key))
                {
                    _diagnostics.Add(Diagnostic.Create(InilynDiagnostics.Unreachable,
                        s.Line, s.Column, s.EndLine, s.EndColumn, FileNameOf(groupEntry.Key, s.Name), s.Name));
                }

                result.Add(new InilynSectionAnalysis(
                    groupEntry.Key,
                    s.Name,
                    kind,
                    _types.TryGetValue(key, out var t) ? [.. t] : [],
                    isReachable));
            }
        }

        return new InilynAnalysis(result, _diagnostics);
    }

    private static bool MatchesType(InilynRuleGroup group, string sectionType, string fromType)
    {
        if (string.Equals(sectionType, fromType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var def = group.GetDefinition(sectionType);
        while (def?.Base is { } baseName)
        {
            if (string.Equals(baseName, fromType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            def = group.GetDefinition(baseName);
        }

        return false;
    }

    /// <summary>
    /// 将引用值拆分为候选目标名（按声明分隔符，或按逗号/空格兜底）。
    /// </summary>
    private static string[] GetReferenceTargets(string value, string? list)
    {
        if (list is not null)
        {
            return value.Split(list, StringSplitOptions.TrimEntries);
        }

        if (value.Contains(','))
        {
            return value.Split(',', StringSplitOptions.TrimEntries);
        }

        if (value.Contains(' '))
        {
            return value.Split(' ', StringSplitOptions.TrimEntries);
        }

        return [value.Trim()];
    }

    private static (string Group, string Type) SplitDotted(string raw, string defaultGroup)
    {
        int dot = raw.IndexOf('.');
        if (dot > 0 && dot < raw.Length - 1)
        {
            return (raw[..dot], raw[(dot + 1)..]);
        }

        return (defaultGroup, raw);
    }

    private static List<ParsedIniSection> ParseSections(string content, string fileName)
    {
        SourceText sourceText = SourceText.Create(content, fileName);
        var tree = IniParser.Parse(sourceText);
        List<ParsedIniSection> result = [];

        if (tree.Root is not IniCompilationUnit unit)
        {
            return result;
        }

        foreach (var entry in unit.Entries)
        {
            if (entry is not IniSectionDecl section)
            {
                continue;
            }

            ParsedIniSection parsed = new() { Name = section.Name.Text };
            (parsed.Line, parsed.Column) = sourceText.GetPosition(section.Start);
            (parsed.EndLine, parsed.EndColumn) = sourceText.GetPosition(section.End);

            foreach (var child in section.Children)
            {
                if (child is not IniKeyValueEntry kv)
                {
                    continue;
                }

                parsed.Keys[kv.Key.Text] = kv.Value.Text;
                var (keyLine, keyColumn) = sourceText.GetPosition(kv.Start);
                var (keyEndLine, keyEndColumn) = sourceText.GetPosition(kv.End);
                parsed.KeyPositions[kv.Key.Text] = (keyLine, keyColumn, keyEndLine, keyEndColumn);
            }

            result.Add(parsed);
        }

        return result;
    }

    /// <summary>
    /// 解析后的 INI 节。
    /// </summary>
    internal sealed class ParsedIniSection
    {
        public string Name { get; set; } = string.Empty;

        public int Line { get; set; }

        public int Column { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public Dictionary<string, string> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, (int Line, int Column, int EndLine, int EndColumn)> KeyPositions { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class SecKeyEqualityComparer : IEqualityComparer<(string Group, string Name)>
    {
        public bool Equals((string Group, string Name) x, (string Group, string Name) y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x.Group, y.Group)
                && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
        }

        public int GetHashCode((string Group, string Name) obj)
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Group),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name));
        }
    }
}
