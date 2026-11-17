using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 可达性 Pass：从 Global / Registry / EnumSection 根出发，沿引用做 BFS，标记 <see cref="EntityModel.IsReachable"/>。
/// </summary>
/// <remarks>
/// 每次运行都会先清空全部标记再重新计算，因此可以在引用图变化后重复调用。
/// 系统级引用（<see cref="ReferenceModel.From"/> 为 <see langword="null"/>）视为从目标自身出发的边。
/// </remarks>
public sealed class ReachabilityPass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "Reachability";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        List<EntityModel> entities = [.. context.Entities];
        Dictionary<EntityId, EntityModel> byId = [];
        Dictionary<PropertyId, EntityId> propertyOwners = [];

        foreach (var entity in entities)
        {
            entity.IsReachable = false;
            byId[entity.Id] = entity;
            foreach (var property in entity.Properties)
                propertyOwners[property.Id] = entity.Id;
        }

        Dictionary<EntityId, List<EntityId>> edges = [];
        foreach (var reference in context.References)
        {
            EntityId source = reference.From is { } from && propertyOwners.TryGetValue(from, out var owner)
                ? owner
                : reference.To;

            if (edges.TryGetValue(source, out var targets) is false)
            {
                targets = [];
                edges[source] = targets;
            }

            if (targets.Contains(reference.To) is false)
                targets.Add(reference.To);
        }

        Queue<EntityId> queue = [];
        foreach (var entity in entities)
        {
            if (entity.Kind is not (EntityKind.Global or EntityKind.Registry or EntityKind.EnumSection))
                continue;

            entity.IsReachable = true;
            queue.Enqueue(entity.Id);
        }

        while (queue.TryDequeue(out var current))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (edges.TryGetValue(current, out var targets) is false)
                continue;

            foreach (var target in targets)
            {
                if (byId.TryGetValue(target, out var entity) is false || entity.IsReachable)
                    continue;

                entity.IsReachable = true;
                queue.Enqueue(target);
            }
        }
    }
}