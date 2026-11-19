using Shimakaze.Sdk.Analyzer.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Analyzer.Common.Passes;

/// <summary>
/// 分类分析器：按规则组给出实体的语义分类，并登记实体拥有的类型。
/// </summary>
/// <remarks>
/// 分类优先级：Global → Registry → EnumSection → Entity（由登记表成员匹配得到）→ Unknown。
/// 未被登记表匹配的实体保留 <see cref="EntitySemanticKind.Unknown"/>，交给发现分析器继续推断类型。
/// </remarks>
public sealed class ClassificationAnalyzer : IAnalyzer
{
    /// <inheritdoc/>
    public string Name => "Classification";

    /// <inheritdoc/>
    public void Analyze(AnalyzerContext context)
    {
        if (context.Rules is not { } rules)
            return;

        Dictionary<(string Group, string Name), EntityModel> byKey = [with(new EntityKeyComparer())];
        foreach (var entity in context.Model.Entities)
            byKey[(entity.Group, entity.Name)] = entity;

        foreach (var entity in context.Model.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            RuleGroup? group = rules.FindGroup(entity.Group);
            (EntitySemanticKind kind, string? typeName) = group is null
                ? (EntitySemanticKind.Unknown, null)
                : Classify(group, entity, byKey);

            context.Annotations.SetKind(entity.Id, kind);
            if (typeName is not null)
                context.Annotations.AddType(entity.Id, typeName);
        }
    }

    private static (EntitySemanticKind Kind, string? TypeName) Classify(
        RuleGroup group,
        EntityModel entity,
        Dictionary<(string Group, string Name), EntityModel> byKey)
    {
        if (group.Globals.TryGetValue(entity.Name, out var global))
            return (EntitySemanticKind.Global, global.Type ?? global.Section);

        if (group.Registries.TryGetValue(entity.Name, out var selfRegistry))
            return (EntitySemanticKind.Registry, selfRegistry.Element);

        if (group.EnumSections.TryGetValue(entity.Name, out var enumSection))
            return (EntitySemanticKind.EnumSection, enumSection.Enum ?? entity.Name);

        foreach ((string registryName, RegistryRule registry) in group.Registries)
        {
            if (byKey.TryGetValue((entity.Group, registryName), out var registryEntity) is false)
                continue;

            if (IsRegistered(registryEntity, entity.Name))
            {
                (string _, string targetType) = SplitReference(registry.Element, entity.Group);
                return (EntitySemanticKind.Entity, targetType);
            }
        }

        return (EntitySemanticKind.Unknown, null);
    }

    /// <summary>
    /// 登记表成员匹配：登记属性值按列表分隔符拆分后逐项比较，忽略大小写。
    /// </summary>
    internal static bool IsRegistered(EntityModel registryEntity, string name)
    {
        foreach (var property in registryEntity.Properties)
        {
            string[] items = property.Value.Split(
                property.ListSeparator is null ? string.Empty : property.ListSeparator,
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in items)
            {
                if (string.Equals(item, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 解析 <c>Group.Name</c> 形式的元素引用；没有组前缀时使用当前组。
    /// </summary>
    internal static (string Group, string Name) SplitReference(string value, string currentGroup)
    {
        int dot = value.IndexOf('.');
        return dot > 0 && dot < value.Length - 1
            ? (value[..dot], value[(dot + 1)..])
            : (currentGroup, value);
    }
}
