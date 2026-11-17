using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 分类 Pass：按规则组标记实体的 <see cref="EntityKind"/>，并登记实体拥有的类型。
/// </summary>
/// <remarks>
/// 分类优先级：Global → Registry → EnumSection → Entity（由注册表成员匹配得到）→ Unknown。
/// 未被注册表登记的实体保留 <see cref="EntityKind.Unknown"/>，交给发现规则 Pass 继续推断类型。
/// </remarks>
public sealed class ClassificationPass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "Classification";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        RuleSet? rules = context.Rules;
        if (rules is null)
            return;

        Dictionary<(string Group, string Name), EntityModel> byKey = new(new EntityKeyComparer());
        foreach (var entity in context.Entities)
            byKey[(entity.Group, entity.Name)] = entity;

        List<EntityModel> entities = [];
        foreach (var entity in context.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            RuleGroup? group = rules.FindGroup(entity.Group);
            (EntityKind kind, string? typeName) = group is null
                ? (EntityKind.Unknown, null)
                : Classify(group, entity, byKey);

            if (typeName is not null)
                context.AnnotateType(entity.Id, typeName);

            entities.Add(CopyWith(entity, kind, typeName));
        }

        context.SetEntities(entities);
    }

    private static (EntityKind Kind, string? TypeName) Classify(
        RuleGroup group,
        EntityModel entity,
        Dictionary<(string Group, string Name), EntityModel> byKey)
    {
        if (group.Globals.TryGetValue(entity.Name, out var global))
            return (EntityKind.Global, global.Type ?? global.Section);

        if (group.Registries.TryGetValue(entity.Name, out var selfRegistry))
            return (EntityKind.Registry, selfRegistry.Element);

        if (group.EnumSections.TryGetValue(entity.Name, out var enumSection))
            return (EntityKind.EnumSection, enumSection.Enum ?? entity.Name);

        foreach ((string registryName, RegistryRule registry) in group.Registries)
        {
            if (byKey.TryGetValue((entity.Group, registryName), out var registryEntity) is false)
                continue;

            if (IsRegistered(registryEntity, entity.Name))
            {
                (string _, string targetType) = SplitReference(registry.Element, entity.Group);
                return (EntityKind.Entity, targetType);
            }
        }

        return (EntityKind.Unknown, null);
    }

    /// <summary>
    /// 注册表成员匹配：注册表属性值按列表分隔符拆分后逐项比较，忽略大小写。
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

    private static EntityModel CopyWith(EntityModel source, EntityKind kind, string? typeName)
        => new()
        {
            Id = source.Id,
            Group = source.Group,
            Name = source.Name,
            Kind = kind,
            TypeName = typeName,
            BaseNames = source.BaseNames,
            Properties = source.Properties,
            Origin = source.Origin,
            IsReachable = source.IsReachable,
        };
}