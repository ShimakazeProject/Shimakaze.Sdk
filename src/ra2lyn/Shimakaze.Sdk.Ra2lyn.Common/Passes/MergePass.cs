using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 合并 Pass：按前端结果顺序合并同组同名实体。
/// </summary>
/// <remarks>
/// 语义固定为：后出现的实体覆盖先出现的同名属性（沿用先出现属性的位置），但覆盖时保留**新值的新属性 ID**，
/// 以便后续引用（<see cref="PropertyId"/>）始终指向最终生效的属性；新出现的属性追加到末尾。
/// 后出现的实体若带有新的继承/混合名，则追加到基名列表；实体的源位置与组以先出现的为准。
/// </remarks>
public sealed class MergePass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "Merge";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        List<EntityModel> merged = [];
        Dictionary<(string Group, string Name), EntityModel> byKey = new(new EntityKeyComparer());

        foreach (var frontend in context.Frontends)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            foreach (var entity in frontend.Entities)
            {
                (string Group, string Name) key = (entity.Group, entity.Name);
                if (byKey.TryGetValue(key, out var existing) is false)
                {
                    merged.Add(CopyEntity(entity, entity.Properties));
                    byKey[key] = entity;
                    continue;
                }

                List<PropertyModel> properties = [.. byKey[key].Properties];
                MergeProperties(properties, entity.Properties);
                List<string> baseNames = [.. byKey[key].BaseNames];
                foreach (string baseName in entity.BaseNames)
                {
                    if (baseNames.Contains(baseName, StringComparer.OrdinalIgnoreCase) is false)
                        baseNames.Add(baseName);
                }

                byKey[key] = CreateCopy(existing, properties, baseNames);
            }
        }

        // 合并结果按首次出现顺序排列，ID 保持首次分配值。
        context.SetEntities(merged.Select(entity => byKey[(entity.Group, entity.Name)]).ToList());
    }

    private static void MergeProperties(List<PropertyModel> target, IReadOnlyList<PropertyModel> incoming)
    {
        foreach (var property in incoming)
        {
            int index = target.FindIndex(item => string.Equals(item.Key, property.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                target[index] = property;
                continue;
            }

            target.Add(property);
        }
    }

    private static EntityModel CopyEntity(EntityModel source, IReadOnlyList<PropertyModel> properties)
        => CreateCopy(source, [.. properties], [.. source.BaseNames]);

    private static EntityModel CreateCopy(EntityModel source, List<PropertyModel> properties, List<string> baseNames)
        => new()
        {
            Id = source.Id,
            Group = source.Group,
            Name = source.Name,
            Kind = source.Kind,
            TypeName = source.TypeName,
            BaseNames = baseNames,
            Properties = properties,
            Origin = source.Origin,
            IsReachable = source.IsReachable,
        };
}

