namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 规则组（定义按组隔离）。
/// </summary>
/// <param name="name">组名。</param>
public sealed class InilynRuleGroup(string name)
{
    private readonly Dictionary<string, InilynSectionDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InilynRegistryDeclaration> _registries = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InilynEnumSectionDeclaration> _enumSections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InilynGlobalDeclaration> _globals = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<InilynDiscoveryRule> _discoveries = [];

    /// <summary>
    /// 组名（单数，如 <c>Rule</c>/<c>Art</c>/<c>Sound</c>）。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 节定义表（类型名 → 定义）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynSectionDefinition> Definitions => _definitions;

    /// <summary>
    /// 注册表声明表（节名 → 声明）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynRegistryDeclaration> Registries => _registries;

    /// <summary>
    /// 枚举节声明表（节名 → 声明）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynEnumSectionDeclaration> EnumSections => _enumSections;

    /// <summary>
    /// 全局节声明表（节名 → 声明）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynGlobalDeclaration> Globals => _globals;

    /// <summary>
    /// 发现规则列表。
    /// </summary>
    public IReadOnlyList<InilynDiscoveryRule> Discoveries => _discoveries;

    /// <summary>
    /// 合并另一个组的定义（多平台时按名合并）。
    /// </summary>
    /// <param name="other">另一个组。</param>
    internal void MergeFrom(InilynRuleGroup other)
    {
        foreach (var (name, def) in other._definitions)
        {
            if (_definitions.TryGetValue(name, out var existing))
            {
                existing.MergeFrom(def);
            }
            else
            {
                _definitions[name] = def;
            }
        }

        foreach (var (section, r) in other._registries)
        {
            _registries[section] = r;
        }

        foreach (var (section, e) in other._enumSections)
        {
            _enumSections[section] = e;
        }

        foreach (var (section, g) in other._globals)
        {
            _globals[section] = g;
        }

        _discoveries.AddRange(other._discoveries);
    }

    /// <summary>
    /// 获取节定义。
    /// </summary>
    /// <param name="typeName">节定义名。</param>
    /// <returns>节定义，不存在时返回 <see langword="null"/>。</returns>
    public InilynSectionDefinition? GetDefinition(string typeName)
    {
        return _definitions.GetValueOrDefault(typeName);
    }

    /// <summary>
    /// 获取注册表声明。
    /// </summary>
    /// <param name="section">节名。</param>
    /// <returns>声明，不存在时返回 <see langword="null"/>。</returns>
    public InilynRegistryDeclaration? GetRegistry(string section)
    {
        return _registries.GetValueOrDefault(section);
    }

    /// <summary>
    /// 获取枚举节声明。
    /// </summary>
    /// <param name="section">节名。</param>
    /// <returns>声明，不存在时返回 <see langword="null"/>。</returns>
    public InilynEnumSectionDeclaration? GetEnumSection(string section)
    {
        return _enumSections.GetValueOrDefault(section);
    }

    /// <summary>
    /// 获取全局节声明。
    /// </summary>
    /// <param name="section">节名。</param>
    /// <returns>声明，不存在时返回 <see langword="null"/>。</returns>
    public InilynGlobalDeclaration? GetGlobal(string section)
    {
        return _globals.GetValueOrDefault(section);
    }

    internal void AddDefinition(InilynSectionDefinition def)
    {
        if (_definitions.TryGetValue(def.Name, out var existing))
        {
            existing.MergeFrom(def);
        }
        else
        {
            _definitions[def.Name] = def;
        }
    }

    internal void AddRegistry(InilynRegistryDeclaration r) => _registries[r.Section] = r;
    internal void AddEnumSection(InilynEnumSectionDeclaration e) => _enumSections[e.Section] = e;
    internal void AddGlobal(InilynGlobalDeclaration g) => _globals[g.Section] = g;
    internal void AddDiscovery(InilynDiscoveryRule d) => _discoveries.Add(d);
}
