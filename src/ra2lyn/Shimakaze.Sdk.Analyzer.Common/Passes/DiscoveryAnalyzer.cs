using System.Globalization;

using Shimakaze.Sdk.Analyzer.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Analyzer.Common.Passes;

/// <summary>
/// 发现分析器：按 <see cref="DiscoverRule"/> 从已知类型实体的键值出发，发现目标类型实体，迭代到不动点。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description><see cref="DiscoverRule.From"/> 可以是类型名，实体拥有的类型沿 <c>Base</c> 链向上匹配即满足。</description></item>
/// <item><description><see cref="DiscoverRule.ResolveKey"/> 不含 <c>#</c> 时按键名精确匹配；含 <c>#</c> 时按 <c>Min</c>/<c>Max</c> 展开（数字或键名）。</description></item>
/// <item><description><see cref="DiscoverRule.Target"/> 支持 <c>Group.Name</c> 跨组引用；目标实体原本未知时被标记为普通实体并补充类型名。</description></item>
/// <item><description><see cref="DiscoverRule.Fallback"/> 在解析失败时作为兜底目标。</description></item>
/// </list>
/// </remarks>
public sealed class DiscoveryAnalyzer : IAnalyzer
{
    /// <inheritdoc/>
    public string Name => "Discovery";

    /// <inheritdoc/>
    public void Analyze(AnalyzerContext context)
    {
        if (context.Rules is not { } rules)
            return;

        Dictionary<(string Group, string Name), EntityModel> byKey = new(new EntityKeyComparer());
        foreach (var entity in context.Model.Entities)
            byKey[(entity.Group, entity.Name)] = entity;

        List<ReferenceModel> references = [.. context.Graph.References];
        HashSet<ReferenceModel> seen = [.. references];

        bool changed = true;
        while (changed)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            changed = false;

            foreach (var entity in context.Model.Entities)
            {
                RuleGroup? group = rules.FindGroup(entity.Group);
                if (group is null
                    || context.Annotations.GetKind(entity.Id) is EntitySemanticKind.Unknown or EntitySemanticKind.Registry)
                {
                    continue;
                }

                foreach (var discoverRule in group.Discover)
                {
                    if (MatchesFrom(rules, context, entity, discoverRule.From) is false)
                        continue;

                    foreach (var property in CollectResolveKeys(entity, discoverRule))
                    {
                        string target = property.Value.Trim();
                        (string targetGroup, string targetType) = ClassificationAnalyzer.SplitReference(discoverRule.Target, entity.Group);

                        EntityModel? found = FindTarget(byKey, targetGroup, target);
                        if (found is null)
                        {
                            if (discoverRule.Fallback is not { Length: > 0 } fallback)
                            {
                                context.Report(
                                    DiagnosticCodes.UnresolvedReference,
                                    DiagnosticSeverity.Warning,
                                    $"发现规则 '{discoverRule.ResolveKey}' 无法解析引用 '{target}'",
                                    property.Origin);
                                continue;
                            }

                            (targetGroup, targetType) = ClassificationAnalyzer.SplitReference(fallback, entity.Group);
                            found = FindTarget(byKey, targetGroup, fallback);
                            if (found is null)
                            {
                                context.Report(
                                    DiagnosticCodes.UnresolvedReference,
                                    DiagnosticSeverity.Warning,
                                    $"发现规则的兜底目标 '{fallback}' 不存在",
                                    property.Origin);
                                continue;
                            }
                        }

                        ReferenceModel reference = new(property.Id, found.Id, ReferenceKind.Derived);
                        if (seen.Add(reference))
                        {
                            references.Add(reference);
                            changed |= TryAnnotate(context, found, targetType);
                        }
                    }
                }
            }
        }

        context.Graph.SetReferences(Order(references, context.Model.Entities));
    }

    /// <summary>
    /// 收集本条发现规则在本实体上要解析的属性。
    /// </summary>
    private static IEnumerable<PropertyModel> CollectResolveKeys(EntityModel entity, DiscoverRule discoverRule)
    {
        string? resolveKey = discoverRule.ResolveKey;
        if (string.IsNullOrWhiteSpace(resolveKey))
            return [];

        if (resolveKey.Contains('#'))
            return CollectWildcard(entity, discoverRule, resolveKey);

        return entity.Properties.Where(property => string.Equals(property.Key, resolveKey, StringComparison.OrdinalIgnoreCase));
    }

    private static List<PropertyModel> CollectWildcard(EntityModel entity, DiscoverRule discoverRule, string resolveKey)
    {
        int min = ResolveBoundary(entity, discoverRule.Min) ?? 0;
        int max = ResolveBoundary(entity, discoverRule.Max) ?? min;

        List<PropertyModel> result = [];
        for (int index = min; index <= max; index++)
        {
            string key = resolveKey.Replace("#", index.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
            foreach (var property in entity.Properties)
            {
                if (string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(property);
                    break;
                }
            }
        }

        return result;
    }

    private static int? ResolveBoundary(EntityModel entity, string? boundary)
    {
        if (boundary is null)
            return null;

        if (int.TryParse(boundary, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            return parsed;

        foreach (var property in entity.Properties)
        {
            if (string.Equals(property.Key, boundary, StringComparison.OrdinalIgnoreCase))
                return int.TryParse(property.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int fromKey) ? fromKey : null;
        }

        return null;
    }

    /// <summary>
    /// 判断实体是否拥有 <paramref name="fromType"/>；<paramref name="fromType"/> 为 <see langword="null"/> 时不做限制。
    /// </summary>
    private static bool MatchesFrom(RuleSet rules, AnalyzerContext context, EntityModel entity, string? fromType)
    {
        if (string.IsNullOrWhiteSpace(fromType))
            return true;

        IReadOnlyList<string> types = context.Annotations.GetTypes(entity.Id);
        foreach (string type in types)
        {
            foreach (string candidate in ExpandTypes(rules, entity.Group, type))
            {
                if (string.Equals(candidate, fromType, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> ExpandTypes(RuleSet rules, string groupName, string type)
    {
        HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
        yield return type;

        SectionDefinitionRule? definition = RuleSetLookup.FindDefinition(rules, groupName, type);
        while (definition?.Base is { } baseName && visited.Add(baseName))
        {
            yield return baseName;
            definition = RuleSetLookup.FindDefinition(rules, groupName, baseName);
        }
    }

    private static EntityModel? FindTarget(Dictionary<(string Group, string Name), EntityModel> byKey, string group, string name)
        => byKey.TryGetValue((group, name), out var found) ? found : null;

    private static bool TryAnnotate(AnalyzerContext context, EntityModel entity, string typeName)
    {
        if (context.Annotations.GetKind(entity.Id) is EntitySemanticKind.Unknown)
            context.Annotations.SetKind(entity.Id, EntitySemanticKind.Entity);
        else if (context.Annotations.GetKind(entity.Id) is not EntitySemanticKind.Entity)
            return false;

        bool isNew = context.Annotations.GetTypes(entity.Id).Contains(typeName, StringComparer.OrdinalIgnoreCase) is false;
        if (isNew)
            context.Annotations.AddType(entity.Id, typeName);

        return isNew;
    }

    /// <summary>
    /// 引用按来源属性顺序排序，入口引用排在最前。
    /// </summary>
    private static List<ReferenceModel> Order(List<ReferenceModel> references, IReadOnlyList<EntityModel> entities)
    {
        Dictionary<PropertyId, int> propertyOrder = [];
        foreach (var entity in entities)
        {
            foreach (var property in entity.Properties)
                propertyOrder[property.Id] = propertyOrder.Count;
        }

        List<ReferenceModel> ordered = [.. references];
        ordered.Sort((left, right) =>
        {
            int leftOrder = left.From is { } from && propertyOrder.TryGetValue(from, out int order) ? order : -1;
            int rightOrder = right.From is { } otherFrom && propertyOrder.TryGetValue(otherFrom, out int otherOrder) ? otherOrder : -1;
            int compare = leftOrder.CompareTo(rightOrder);
            if (compare is not 0)
                return compare;

            compare = left.To.CompareTo(right.To);
            return compare is not 0 ? compare : left.Kind.CompareTo(right.Kind);
        });

        return ordered;
    }
}
