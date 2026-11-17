using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 引用 Pass：根据实体的类型与键规则解析属性值中的实体引用。
/// </summary>
/// <remarks>
/// 引用来源：
/// <list type="bullet">
/// <item><description>类型的键规则声明为已知节定义（实体类型）时，属性值中的每个元素都是对该实体的 <see cref="ReferenceKind.SectionRef"/> 引用。</description></item>
/// <item><description>注册表实体的属性值登记成员，成为 <see cref="ReferenceKind.RegistryRef"/> 引用。</description></item>
/// <item><description>全局实体自身产生一条系统级 <see cref="ReferenceKind.Global"/> 引用。</description></item>
/// </list>
/// 无法解析的目标只报 <see cref="DiagnosticCodes.UnresolvedReference"/>，不生成伪边。
/// </remarks>
public sealed class ReferencePass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "Reference";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        RuleSet? rules = context.Rules;
        if (rules is null)
            return;

        Dictionary<(string Group, string Name), EntityModel> byKey = new(new EntityKeyComparer());
        Dictionary<PropertyId, EntityId> owners = [];
        foreach (var entity in context.Entities)
        {
            byKey[(entity.Group, entity.Name)] = entity;
            foreach (var property in entity.Properties)
                owners[property.Id] = entity.Id;
        }

        List<ReferenceModel> references = [];
        foreach (var entity in context.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            RuleGroup? group = rules.FindGroup(entity.Group);
            switch (entity.Kind)
            {
                case EntityKind.Global:
                    references.Add(new ReferenceModel(null, entity.Id, ReferenceKind.Global));
                    CollectFromProperties(context, rules, group, entity, byKey, references);
                    break;
                case EntityKind.Registry:
                    CollectRegistryMembers(context, rules, entity, group, byKey, references);
                    break;
                case EntityKind.EnumSection:
                    break;
                case EntityKind.Unknown:
                case EntityKind.Entity:
                    CollectFromProperties(context, rules, group, entity, byKey, references);
                    break;
            }
        }

        context.SetReferences(Deduplicate(references, owners));
    }

    private static void CollectRegistryMembers(
        CompilationContext context,
        RuleSet rules,
        EntityModel entity,
        RuleGroup? group,
        Dictionary<(string Group, string Name), EntityModel> byKey,
        List<ReferenceModel> references)
    {
        _ = rules;

        if (group is null || group.Registries.TryGetValue(entity.Name, out var registry) is false)
            return;

        (string targetGroup, _) = ClassificationPass.SplitReference(registry.Element, entity.Group);
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

                references.Add(new ReferenceModel(property.Id, target.Id, ReferenceKind.RegistryRef));
            }
        }
    }

    private static void CollectFromProperties(
        CompilationContext context,
        RuleSet rules,
        RuleGroup? group,
        EntityModel entity,
        Dictionary<(string Group, string Name), EntityModel> byKey,
        List<ReferenceModel> references)
    {
        if (group is null)
            return;

        foreach (var typeName in context.GetTypes(entity.Id).DefaultIfEmpty(entity.TypeName).OfType<string>())
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

                    references.Add(new ReferenceModel(property.Id, target.Id, ReferenceKind.SectionRef));
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

    private static void ReportUnresolved(CompilationContext context, PropertyModel property, string name)
        => context.Diagnostics.Add(new Diagnostic(
            DiagnosticCodes.UnresolvedReference,
            DiagnosticSeverity.Warning,
            $"无法解析的引用 '{name}'",
            property.Origin));

    /// <summary>
    /// 去重并按来源实体顺序排序；系统级引用（<see cref="ReferenceModel.From"/> 为 <see langword="null"/>）排在对应实体最前。
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
