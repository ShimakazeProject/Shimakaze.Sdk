using Microsoft.EntityFrameworkCore;

using Shimakaze.Sdk.Inilyn.Data;
using Shimakaze.Sdk.Inilyn.Data.Semantic;
using Shimakaze.Sdk.Inilyn.Data.Syntax;
using Shimakaze.Sdk.Inilyn.Model;

namespace Shimakaze.Sdk.Inilyn.Semantic;

/// <summary>
/// 使用 Inilyn XML 规则集分析已持久化的 INI 文档。
/// </summary>
public sealed class SemanticRunner(IniDbContext db)
{
    /// <summary>
    /// 加载规则并重新生成节语义与引用关系。
    /// </summary>
    /// <param name="rulePath">规则集入口 XML 文件路径。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task RunAsync(string rulePath, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rulePath);

        var rules = await RuleLoader.LoadAsync(rulePath);
        var sections = await db.Sections
            .Include(section => section.Document)
            .ThenInclude(document => document!.Category)
            .Include(section => section.KeyValues)
            .OrderBy(section => section.DocumentId)
            .ThenBy(section => section.Order)
            .ToListAsync(ct);

        db.SectionReferences.RemoveRange(db.SectionReferences);
        db.SectionSemantics.RemoveRange(db.SectionSemantics);
        db.SectionTypeInfos.RemoveRange(db.SectionTypeInfos);
        await db.SaveChangesAsync(ct);

        Dictionary<(string Group, string Name), SectionNode> sectionByName = new(new SectionKeyComparer());
        foreach (var section in sections)
            sectionByName[(section.Document.Category.Name, section.Name)] = section;


        List<SectionSemanticInfo> semantics = [];
        List<SectionReference> references = [];
        List<SectionTypeInfo> typeAssignments = [];
        Dictionary<Guid, List<string>> typesBySection = [];

        foreach (var section in sections)
        {
            ct.ThrowIfCancellationRequested();

            string groupName = section.Document.Category.Name;
            var group = rules.Groups.GetValueOrDefault(groupName);
            if (group is null)
            {
                semantics.Add(CreateSemantic(section, groupName, SectionKind.Unknown, null, false));
                continue;
            }

            if (group.Registries.TryGetValue(section.Name, out var registry))
            {
                semantics.Add(CreateSemantic(section, groupName, SectionKind.Registry, null, true));
                (string targetGroup, _) = SplitReference(registry.Element, groupName);
                foreach (var kv in section.KeyValues)
                {
                    if (kv.Value is null) continue;
                    string[] names = kv.Value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    foreach (string name in names)
                    {
                        if (sectionByName.TryGetValue((targetGroup, name), out var target))
                        {
                            references.Add(new SectionReference
                            {
                                SourceKeyValueId = kv.Id,
                                TargetSectionId = target.Id,
                                ReferenceKind = ReferenceKind.RegistryRef,
                            });
                        }
                    }
                }
                continue;
            }

            if (group.Globals.TryGetValue(section.Name, out var global))
            {
                string sectionType = global.Type ?? global.Section;
                semantics.Add(CreateSemantic(section, groupName, SectionKind.Global, sectionType, true));
                if (!typesBySection.TryGetValue(section.Id, out var tlist))
                {
                    tlist = [];
                    typesBySection[section.Id] = tlist;
                }
                if (!tlist.Contains(sectionType, StringComparer.OrdinalIgnoreCase))
                    tlist.Add(sectionType);
                references.Add(new SectionReference
                {
                    TargetSectionId = section.Id,
                    ReferenceKind = ReferenceKind.Global,
                });
                continue;
            }

            if (group.EnumSections.ContainsKey(section.Name))
            {
                semantics.Add(CreateSemantic(section, groupName, SectionKind.EnumSection, null, true));
                continue;
            }

            var sectionTypes = FindRegistryTypes(section, groupName, rules, sectionByName);
            if (sectionTypes.Count == 0)
            {
                semantics.Add(CreateSemantic(section, groupName, SectionKind.Unknown, null, false));
                continue;
            }

            semantics.Add(CreateSemantic(section, groupName, SectionKind.Entity, sectionTypes[0], false));
            foreach (string t in sectionTypes)
            {
                typeAssignments.Add(new SectionTypeInfo { SectionId = section.Id, TypeName = t });
            }
        }

        Dictionary<Guid, SectionNode> sectionById = [];
        foreach (var s in sections)
            sectionById[s.Id] = s;

        Dictionary<Guid, SectionSemanticInfo> semanticById = [];
        foreach (var sem in semantics)
            semanticById[sem.SectionId] = sem;

        // 节 -> 所有已分配类型（注册表匹配 + 后续 Discover 分类都会追加到这里）
        foreach (var ta in typeAssignments)
        {
            if (!typesBySection.TryGetValue(ta.SectionId, out var list))
            {
                list = [];
                typesBySection[ta.SectionId] = list;
            }
            list.Add(ta.TypeName);
        }

        // 通过 Discover 规则迭代发现：在匹配 From 类型的节上，ResolveKey 键的值是 Target 类型的节
        bool changed;
        do
        {
            changed = false;
            foreach (var (groupName, group) in rules.Groups)
            {
                foreach (var disc in group.Discover)
                {
                    if (string.IsNullOrWhiteSpace(disc.From) || string.IsNullOrWhiteSpace(disc.ResolveKey))
                        continue;

                    (string targetGroup, string targetType) = SplitReference(disc.Target, groupName);
                    foreach (var sem in semantics)
                    {
                        if (sem.SectionKind is SectionKind.Unknown or SectionKind.Registry)
                            continue;
                        if (!string.Equals(sem.GroupName, groupName, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (!sectionById.TryGetValue(sem.SectionId, out var node))
                            continue;
                        if (!typesBySection.TryGetValue(sem.SectionId, out var sectionTypeList) || !sectionTypeList.Any(t => MatchesType(group, t, disc.From)))
                            continue;

                        // 收集需要处理的键值对（支持 # 通配：Weapon# + Min/Max → Weapon1..WeaponN）
                        var matchingKVs = new List<(string ActualKey, string? Value)>();
                        if (disc.ResolveKey.Contains('#'))
                        {
                            // Min 和 Max 均可为数字或键名
                            int minVal = 0;
                            int maxVal = 0;
                            if (int.TryParse(disc.Min, out int parsedMin))
                            {
                                minVal = parsedMin;
                            }
                            else if (disc.Min is not null)
                            {
                                foreach (var kv in node.KeyValues)
                                {
                                    if (string.Equals(kv.Key, disc.Min, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!int.TryParse(kv.Value, out minVal))
                                            minVal = 0;
                                        break;
                                    }
                                }
                            }

                            if (int.TryParse(disc.Max, out int parsedMax))
                            {
                                maxVal = parsedMax;
                            }
                            else if (disc.Max is not null)
                            {
                                foreach (var kv in node.KeyValues)
                                {
                                    if (string.Equals(kv.Key, disc.Max, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!int.TryParse(kv.Value, out maxVal))
                                            maxVal = 0;
                                        break;
                                    }
                                }
                            }

                            if (maxVal > 0)
                            {
                                for (int i = minVal; i <= maxVal; i++)
                                {
                                    string actualKey = disc.ResolveKey.Replace("#", i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                    foreach (var kv in node.KeyValues)
                                    {
                                        if (string.Equals(kv.Key, actualKey, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matchingKVs.Add((actualKey, kv.Value));
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 普通键名精确匹配
                            foreach (var kv in node.KeyValues)
                            {
                                if (string.Equals(kv.Key, disc.ResolveKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    matchingKVs.Add((disc.ResolveKey, kv.Value));
                                    break;
                                }
                            }
                        }

                        foreach (var (actualKey, raw) in matchingKVs)
                        {
                            if (raw is null)
                                continue;

                            string? listSep = GetListSeparator(group, sem.SectionType ?? sectionTypeList[0], actualKey);
                            foreach (string targetName in SplitValues(raw, listSep))
                            {
                                if (!sectionByName.TryGetValue((targetGroup, targetName), out var target)) continue;

                                references.Add(new SectionReference
                                {
                                    SourceKeyValueId = FindKeyValueId(node, actualKey),
                                    TargetSectionId = target.Id,
                                    ReferenceKind = ReferenceKind.Discovery,
                                });

                                if (semanticById.TryGetValue(target.Id, out var targetSem) && targetSem.SectionKind == SectionKind.Unknown)
                                {
                                    targetSem.SectionKind = SectionKind.Entity;
                                    targetSem.SectionType = targetType;
                                    if (!typesBySection.TryGetValue(target.Id, out var tlist))
                                    {
                                        tlist = [];
                                        typesBySection[target.Id] = tlist;
                                    }
                                    if (!tlist.Contains(targetType, StringComparer.OrdinalIgnoreCase))
                                    {
                                        tlist.Add(targetType);
                                    }
                                    typeAssignments.Add(new SectionTypeInfo { SectionId = target.Id, TypeName = targetType });
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
        } while (changed);

        // BFS 可达性传播：从已标记为可达的节出发，沿着引用关系传播可达性
        // 1. 构建 KV -> Section 映射
        Dictionary<Guid, Guid> kvToSection = [];
        foreach (var s in sections)
        {
            foreach (var kv in s.KeyValues)
            {
                kvToSection[kv.Id] = s.Id;
            }
        }

        // 2. 构建 Section -> [TargetSectionId] 引用映射
        Dictionary<Guid, List<Guid>> refsBySection = [];
        foreach (var r in references)
        {
            if (r.SourceKeyValueId is null || r.TargetSectionId is null) continue;
            if (!kvToSection.TryGetValue(r.SourceKeyValueId.Value, out var srcSectionId)) continue;
            if (!refsBySection.TryGetValue(srcSectionId, out var targets))
            {
                targets = [];
                refsBySection[srcSectionId] = targets;
            }
            targets.Add(r.TargetSectionId.Value);
        }

        // 3. SectionId -> SemanticInfo 映射已在上面构建

        // 4. BFS：从已可达的节出发，传播可达性
        Queue<Guid> queue = new(semantics.Where(s => s.IsReachable).Select(s => s.SectionId));
        while (queue.TryDequeue(out var sectionId))
        {
            if (!refsBySection.TryGetValue(sectionId, out var targets)) continue;
            foreach (var targetId in targets)
            {
                if (semanticById.TryGetValue(targetId, out var targetSem) && !targetSem.IsReachable)
                {
                    targetSem.IsReachable = true;
                    queue.Enqueue(targetId);
                }
            }
        }

        await db.SectionSemantics.AddRangeAsync(semantics, ct);
        await db.SectionTypeInfos.AddRangeAsync(typeAssignments, ct);
        await db.SectionReferences.AddRangeAsync(references, ct);
        await db.SaveChangesAsync(ct);
    }

    private static SectionSemanticInfo CreateSemantic(SectionNode section, string groupName, SectionKind kind, string? type, bool reachable)
        => new()
        {
            SectionId = section.Id,
            DocumentId = section.DocumentId,
            GroupName = groupName,
            SectionKind = kind,
            SectionType = type,
            IsReachable = reachable,
        };

    private static List<string> FindRegistryTypes(SectionNode section, string groupName, RuleSet rules, Dictionary<(string Group, string Name), SectionNode> sectionByName)
    {
        List<string> result = [];
        foreach ((string registryName, var registry) in rules.Groups[groupName].Registries)
        {
            if (!sectionByName.TryGetValue((groupName, registryName), out var registrySection))
            {
                continue;
            }

            if (registrySection.KeyValues.Any(keyValue => string.Equals(keyValue.Value?.Trim(), section.Name, StringComparison.Ordinal)))
            {
                result.Add(SplitReference(registry.Element, groupName).Name);
            }
        }

        return result;
    }

    private static bool MatchesType(RuleGroup group, string sectionType, string fromType)
    {
        if (string.Equals(sectionType, fromType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var def = group.Definitions.GetValueOrDefault(sectionType);
        while (def?.Base is { } baseName)
        {
            if (string.Equals(baseName, fromType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            def = group.Definitions.GetValueOrDefault(baseName);
        }

        return false;
    }

    private static string? GetListSeparator(RuleGroup group, string sectionType, string keyName)
    {
        if (!group.Definitions.TryGetValue(sectionType, out var definition)) return null;
        var keys = definition.GetEffectiveKeys(group);
        return keys.TryGetValue(keyName, out var key) ? key.List : null;
    }

    private static Guid? FindKeyValueId(SectionNode section, string keyName)
    {
        foreach (var kv in section.KeyValues)
        {
            if (string.Equals(kv.Key, keyName, StringComparison.OrdinalIgnoreCase))
            {
                return kv.Id;
            }
        }

        return null;
    }

    private static string[] SplitValues(string value, string? separator)
        => separator is null
        ? [value]
        : value.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static (string Group, string Name) SplitReference(string value, string currentGroup)
    {
        int dot = value.IndexOf('.');
        return dot > 0 && dot < value.Length - 1
            ? (value[..dot], value[(dot + 1)..])
            : (currentGroup, value);
    }
}

file sealed class SectionKeyComparer : IEqualityComparer<(string Group, string Name)>
{
    public bool Equals((string Group, string Name) x, (string Group, string Name) y)
        => string.Equals(x.Group, y.Group, StringComparison.OrdinalIgnoreCase)
        && string.Equals(x.Name, y.Name, StringComparison.Ordinal);

    public int GetHashCode((string Group, string Name) value)
        => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(value.Group), StringComparer.Ordinal.GetHashCode(value.Name));
}