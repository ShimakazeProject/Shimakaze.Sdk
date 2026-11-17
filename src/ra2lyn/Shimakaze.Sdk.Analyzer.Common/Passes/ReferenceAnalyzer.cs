using Shimakaze.Sdk.Analyzer.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Analyzer.Common.Passes;

/// <summary>
/// 引用分析器：根据实体语义与键规则解析属性值中的实体引用，结果写入 <see cref="ReferenceGraph"/>。
/// </summary>
/// <remarks>
/// 引用来源：
/// <list type="bullet">
/// <item><description>键规则声明为已知节定义（实体类型）时，属性值中的每个元素都是对该实体的 <see cref="ReferenceKind.Direct"/> 引用。</description></item>
/// <item><description>登记表实体的属性值登记成员，成为 <see cref="ReferenceKind.Indirect"/> 引用。</description></item>
/// <item><description>全局实体自身产生一条 <see cref="ReferenceKind.Entry"/> 引用。</description></item>
/// </list>
/// 无法解析的目标只报 <see cref="DiagnosticCodes.UnresolvedReference"/>，不生成伪边。
/// </remarks>
public sealed class ReferenceAnalyzer : IAnalyzer
{
    /// <inheritdoc/>
    public string Name => "Reference";

    /// <inheritdoc/>
    public void Analyze(AnalyzerContext context)
    {
        if (context.Rules is not { } rules)
            return;

        Dictionary<(string Group, string Name), EntityModel> byKey = new(new EntityKeyComparer());
        Dictionary<PropertyId, EntityId> owners = [];
        foreach (var entity in context.Model.Entities)
        {
            byKey[(entity.Group, entity.Name)] = entity;
            foreach (var property in entity.Properties)
                owners[property.Id] = entity.Id;
        }

        List<ReferenceModel> references = [];
        foreach (var entity in context.Model.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            RuleGroup? group = rules.FindGroup(entity.Group);
            switch (context.Annotations.GetKind(entity.Id))
            {
                case EntitySemanticKind.Global:
                    references.Add(new ReferenceModel(null, entity.Id, ReferenceKind.Entry));
                    CollectFromProperties(context, rules, group, entity, byKey, references);
                    break;
                case EntitySemanticKind.Registry:
                    CollectRegistryMembers(context, entity, group, byKey, references);
                    break;
                case EntitySemanticKind.EnumSection:
                    break;
                case EntitySemanticKind.Unknown:
                case EntitySemanticKind.Entity:
                    CollectFromProperties(context, rules, group, entity, byKey, references);
                    break;
            }
        }

        context.Graph.SetReferences(Deduplicate(references, owners));
    }

    private static void CollectRegistryMembers(
        AnalyzerContext context,
        EntityModel entity,
        RuleGroup? group,
        Dictionary<(string Group, string Name), EntityModel> byKey,
        List<ReferenceModel> references)
    {
        if (group is null || group.Registries.TryGetValue(entity.Name, out var registry) is false)
            return;

        (string targetGroup, _) = ClassificationAnalyzer.SplitReference(registry.Element, entity.Group);
        foreach (var property in entity.Properties)
        {
            foreach (string item in SplitValues(property))
            {
                EntityModel? target = FindEntity(byKey, targetGroup, item);
                if (target is null)
                {
                    ReportUnresolved(context, property, item);
                    continue;
                }

                references.Add(new ReferenceModel(property.Id, target.Id, ReferenceKind.Indirect));
            }
        }
    }

    private static void CollectFromProperties(
        AnalyzerContext context,
        RuleSet rules,
        RuleGroup? group,
        EntityModel entity,
        Dictionary<(string Group, string Name), EntityModel> byKey,
        List<ReferenceModel> references)
    {
        if (group is null)
            return;

        foreach (var typeName in context.Annotations.GetTypes(entity.Id))
        {
            SectionDefinitionRule? definition = RuleSetLookup.FindDefinition(rules, entity.Group, typeName);
            if (definition is null)
                continue;

            Dictionary<string, KeyRule> keys = definition.GetEffectiveKeys(group);
            foreach (var property in entity.Properties)
            {
                if (keys.TryGetValue(property.Key, out var rule) is false)
                    continue;

                // 只有声明为已知节定义的键才承载实体引用；内置类型与外部类型由引用图之外的类型系统负责。
                if (RuleSetLookup.FindDefinition(rules, entity.Group, rule.Type) is null)
                    continue;

                foreach (string item in SplitValues(property))
                {
                    EntityModel? target = FindEntity(byKey, entity.Group, item);
                    if (target is null)
                    {
                        ReportUnresolved(context, property, item);
                        continue;
                    }

                    references.Add(new ReferenceModel(property.Id, target.Id, ReferenceKind.Direct));
                }
            }
        }
    }

    private static string[] SplitValues(PropertyModel property)
    {
        if (property.ListSeparator is not { } separator)
        {
            string trimmed = property.Value.Trim();
            return trimmed.Length is 0 ? [] : [trimmed];
        }

        string[] items = property.Value.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return items;
    }

    private static EntityModel? FindEntity(
        Dictionary<(string Group, string Name), EntityModel> byKey,
        string group,
        string name)
        => byKey.TryGetValue((group, name), out var found) ? found : null;

    private static void ReportUnresolved(AnalyzerContext context, PropertyModel property, string name)
        => context.Report(
            DiagnosticCodes.UnresolvedReference,
            DiagnosticSeverity.Warning,
            $"无法解析的引用 '{name}'",
            property.Origin);

    /// <summary>
    /// 去重并按来源实体顺序排序；入口引用（<see cref="ReferenceModel.From"/> 为 <see langword="null"/>）排在对应实体最前。
    /// </summary>
    private static List<ReferenceModel> Deduplicate(
        List<ReferenceModel> references,
        Dictionary<PropertyId, EntityId> owners)
    {
        HashSet<ReferenceModel> seen = [];
        List<ReferenceModel> distinct = [];

        foreach (var reference in references)
        {
            if (seen.Add(reference))
                distinct.Add(reference);
        }

        List<ReferenceModel> ordered = [.. distinct];
        ordered.Sort((left, right) =>
        {
            int leftOwner = GetOwner(left, owners);
            int rightOwner = GetOwner(right, owners);
            int compare = leftOwner.CompareTo(rightOwner);
            if (compare is not 0)
                return compare;

            compare = CompareNullable(left.From, right.From);
            if (compare is not 0)
                return compare;

            compare = left.To.CompareTo(right.To);
            return compare is not 0 ? compare : left.Kind.CompareTo(right.Kind);
        });

        return ordered;
    }

    private static int GetOwner(ReferenceModel reference, Dictionary<PropertyId, EntityId> owners)
        => reference.From is { } from && owners.TryGetValue(from, out var owner) ? owner.Value : -1;

    private static int CompareNullable(PropertyId? left, PropertyId? right)
        => (left?.Value ?? -1).CompareTo(right?.Value ?? -1);
}
