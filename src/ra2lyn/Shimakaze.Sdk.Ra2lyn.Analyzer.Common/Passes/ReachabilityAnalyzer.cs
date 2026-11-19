using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Passes;

/// <summary>
/// 可达性分析器：从入口类实体出发，沿引用图做 BFS，把结果写入 <see cref="SemanticAnnotations"/>。
/// </summary>
/// <remarks>
/// 每次运行都会先清空全部可达标记再重新计算，因此可以在引用图变化后重复调用。
/// 入口引用（<see cref="ReferenceModel.From"/> 为 <see langword="null"/>）视为从目标自身出发的边。
/// </remarks>
public sealed class ReachabilityAnalyzer : IAnalyzer
{
    /// <inheritdoc/>
    public string Name => "Reachability";

    /// <inheritdoc/>
    public void Analyze(AnalyzerContext context)
    {
        IReadOnlyList<EntityModel> entities = context.Model.Entities;
        Dictionary<EntityId, EntityModel> byId = [];
        Dictionary<PropertyId, EntityId> propertyOwners = [];

        foreach (var entity in entities)
        {
            context.Annotations.SetReachable(entity.Id, false);
            byId[entity.Id] = entity;
            foreach (var property in entity.Properties)
                propertyOwners[property.Id] = entity.Id;
        }

        Dictionary<EntityId, List<EntityId>> edges = [];
        foreach (var reference in context.Graph.References)
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
            if (context.Annotations.GetKind(entity.Id) is not (EntitySemanticKind.Global or EntitySemanticKind.Registry or EntitySemanticKind.EnumSection))
                continue;

            context.Annotations.SetReachable(entity.Id, true);
            queue.Enqueue(entity.Id);
        }

        while (queue.TryDequeue(out var current))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (edges.TryGetValue(current, out var targets) is false)
                continue;

            foreach (var target in targets)
            {
                if (byId.TryGetValue(target, out var entity) is false || context.Annotations.IsReachable(entity.Id))
                    continue;

                context.Annotations.SetReachable(entity.Id, true);
                queue.Enqueue(entity.Id);
            }
        }
    }
}
