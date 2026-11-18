using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 合并 Pass：把同组同名实体按优先级稳定排序后合并。
/// </summary>
/// <remarks>
/// 语义固定为：在合并同一 <c>(Group, Name)</c> 桶之前，先把桶内所有 entity 按
/// <c>(Priority 升序, 原始 FrontendIndex 升序, 原始 EntityIndex 升序)</c> 稳定排序，
/// 然后按顺序依次跑属性合并（TODO.md §1.1.2）。高 priority 的 entity 排在尾部、覆盖先到者。
/// 覆盖时，被覆盖的旧属性值不会被丢弃，而是以 <see cref="PropertyHistoryEntry"/> 的形式
/// 追加到新属性的 <see cref="PropertyModel.History"/> 链中（TODO.md §1.3）；新 key 的 History 为空。
/// 后出现的实体若带有新的继承/混合名，则追加到基名列表；实体的源位置与组以排序后第一个 entity 为准。
/// </remarks>
public sealed class MergePass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "Merge";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        // 1. 收集所有 (Group, Name) 桶，记录每个 entity 的原始来源序号。
        Dictionary<(string Group, string Name), List<BucketEntry>> buckets = new(new EntityKeyComparer());
        List<(string Group, string Name)> bucketOrder = [];

        for (int frontendIndex = 0; frontendIndex < context.Frontends.Count; frontendIndex++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var frontend = context.Frontends[frontendIndex];

            for (int entityIndex = 0; entityIndex < frontend.Entities.Count; entityIndex++)
            {
                var entity = frontend.Entities[entityIndex];
                var key = (entity.Group, entity.Name);
                if (buckets.TryGetValue(key, out var list) is false)
                {
                    list = [];
                    buckets[key] = list;
                    bucketOrder.Add(key);
                }

                list.Add(new BucketEntry(entity, frontendIndex, entityIndex));
            }
        }

        // 2. 对每个桶做稳定排序后依次合并。
        Dictionary<(string Group, string Name), EntityModel> mergedByKey = new(new EntityKeyComparer());
        List<(string Group, string Name)> mergedOrder = [];

        foreach (var key in bucketOrder)
        {
            var bucket = buckets[key];
            // 稳定排序：Priority 升序 → FrontendIndex 升序 → EntityIndex 升序。
            bucket.Sort(static (a, b) =>
            {
                int c = a.Entity.Priority.CompareTo(b.Entity.Priority);
                if (c != 0) return c;
                c = a.FrontendIndex.CompareTo(b.FrontendIndex);
                return c != 0 ? c : a.EntityIndex.CompareTo(b.EntityIndex);
            });

            // 同名模板跨文件 / 跨前端出现：直接报 error，不合并（见 ReportDuplicateTemplates）。
            bool mergingTemplates = ReportDuplicateTemplates(context, key, bucket);

            // 排序后的第一个 entity 决定 Id / Group / Name / Origin。
            var first = bucket[0];
            List<TrackedProperty> properties = [.. first.Entity.Properties
                .Select(p => new TrackedProperty(p, first.Entity.Priority, first.FrontendIndex))];
            List<string> baseNames = [.. first.Entity.BaseNames];

            for (int i = 1; i < bucket.Count; i++)
            {
                var entry = bucket[i];
                if (mergingTemplates is false)
                {
                    // 模板重复：body 一律不合并，避免"静默挑一个"后把错误扩散到本不该匹配的节。
                    // 只有 bucket[0] 这条源文本会继续走 TemplateExpansionPass。
                    continue;
                }

                MergeProperties(properties, entry.Entity.Properties, entry.Entity.Priority, entry.FrontendIndex);
                foreach (string baseName in entry.Entity.BaseNames)
                {
                    if (baseNames.Contains(baseName, StringComparer.OrdinalIgnoreCase) is false)
                        baseNames.Add(baseName);
                }
            }

            EntityModel mergedEntity = CreateCopy(
                first.Entity,
                [.. properties.Select(t => t.Model)],
                baseNames);
            mergedByKey[key] = mergedEntity;
            mergedOrder.Add(key);
        }

        // 合并结果按首次出现顺序排列，ID 保持首次分配值。
        context.SetEntities(mergedOrder.Select(k => mergedByKey[k]).ToList());
    }

    /// <summary>
    /// 检查一个 <c>(Group, Name)</c> 桶里是否有跨文件重复的模板节；有则逐对报 error。
    /// </summary>
    /// <param name="context">编译上下文，用于写诊断。</param>
    /// <param name="key">桶键。</param>
    /// <param name="bucket">已按 <c>(Priority 升序, FrontendIndex 升序, EntityIndex 升序)</c> 排序的桶。</param>
    /// <returns>
    /// 桶内第一个 entity 是模板时返回 <see langword="false"/>（调用方必须跳过 body 合并）；
    /// 不是模板时返回 <see langword="true"/>（普通节照常合并）。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 用户拍板的语义：<b>同名模板跨文件 / 跨前端出现时直接 error，不合并</b>。
    /// 模板的 body 与 <c>where</c> 都不是"属性"，无法用覆盖顺序平铺——静默合并会让哪条 body / 哪个条件生效
    /// 取决于源文件顺序，把错误扩散到本不该匹配的节。因此这里不再尝试任何合并策略，只负责报错。
    /// </para>
    /// <para>
    /// 诊断位置指向<b>后出现</b>的那个模板，消息里列出与它冲突的先前文件（源文件路径取
    /// <see cref="EntityModel.Origin"/>）与所属前端序号。同一前端的多条同名模板由前端降级阶段的
    /// INI101 负责，这里不重复报。
    /// </para>
    /// </remarks>
    private static bool ReportDuplicateTemplates(
        CompilationContext context,
        (string Group, string Name) key,
        List<BucketEntry> bucket)
    {
        var first = bucket[0];
        if (first.Entity.IsTemplate is false)
            return true;

        for (int i = 1; i < bucket.Count; i++)
        {
            var later = bucket[i];
            for (int j = 0; j < i; j++)
            {
                var earlier = bucket[j];

                // 同一前端里的同名模板由 AstToIrLowerer 报 INI101，这里只报跨前端的。
                if (earlier.FrontendIndex == later.FrontendIndex)
                    continue;

                context.Diagnostics.Add(new Diagnostic(
                    DiagnosticCodes.DuplicateTemplate,
                    DiagnosticSeverity.Error,
                    $"模板 '{key.Name}' 重复定义：{DescribeLocation(earlier)} 与 {DescribeLocation(later)}",
                    later.Entity.Origin));
            }
        }

        return false;
    }

    /// <summary>把一个桶条目描述成"文件 + 前端"的可读位置，供重复模板诊断使用。</summary>
    private static string DescribeLocation(BucketEntry entry)
    {
        // 源文件路径直接取自实体位置：它由前端按 <c>SourceFile.Path</c> 填入，与 context.Sources 一致。
        string path = entry.Entity.Origin.Path;
        string file = string.IsNullOrEmpty(path) ? "未知文件" : path;

        return $"文件 '{file}'（前端 #{entry.FrontendIndex}）";
    }

    /// <summary>
    /// 把 incoming 的属性合并到 target。覆盖发生时构造 History 链。
    /// </summary>
    private static void MergeProperties(
        List<TrackedProperty> target,
        IReadOnlyList<PropertyModel> incoming,
        int incomingPriority,
        int incomingFrontendIndex)
    {
        foreach (var property in incoming)
        {
            int index = target.FindIndex(item => string.Equals(item.Model.Key, property.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                // 覆盖：旧值挂到新值的 History 上，保留旧的 History 链。
                var old = target[index];
                List<PropertyHistoryEntry> history = [.. old.Model.History];
                history.Add(new PropertyHistoryEntry(
                    old.Model.Value,
                    old.Model.Origin,
                    old.Priority,
                    old.FrontendIndex,
                    IsAppend: false));
                PropertyModel replaced = new()
                {
                    Id = property.Id,
                    Key = property.Key,
                    Value = property.Value,
                    ValueKind = property.ValueKind,
                    ListSeparator = property.ListSeparator,
                    TemplateName = property.TemplateName,
                    Origin = property.Origin,
                    History = history,
                };
                target[index] = new TrackedProperty(replaced, incomingPriority, incomingFrontendIndex);
                continue;
            }

            // 新 key：History 为空。
            target.Add(new TrackedProperty(property, incomingPriority, incomingFrontendIndex));
        }
    }

    private static EntityModel CreateCopy(EntityModel source, List<PropertyModel> properties, List<string> baseNames)
        => new()
        {
            Id = source.Id,
            Group = source.Group,
            Name = source.Name,
            BaseNames = baseNames,
            Properties = properties,
            Origin = source.Origin,
            Priority = source.Priority,
            // 模板标记必须随合并结果存活到 TemplateExpansionPass（TODO.md §4.5：模板节也参与 MergePass）。
            IsTemplate = source.IsTemplate,
            TemplateWhere = source.TemplateWhere,
            // 表达式位置同样随合并结果保留，供 TemplateExpansionPass 换算 where 求值诊断的 span。
            TemplateWhereSpan = source.TemplateWhereSpan,
        };

    /// <summary>桶内一个 entity 的来源定位。</summary>
    /// <param name="Entity">实体本身。</param>
    /// <param name="FrontendIndex">所属前端的序号。</param>
    /// <param name="EntityIndex">前端内 entity 的序号。</param>
    private readonly record struct BucketEntry(EntityModel Entity, int FrontendIndex, int EntityIndex);

    /// <summary>合并过程中跟踪当前 target 属性所属的 priority / frontendIndex，以便覆盖时写 History。</summary>
    /// <param name="Model">属性模型。</param>
    /// <param name="Priority">该属性值来源 entity 的 Priority。</param>
    /// <param name="FrontendIndex">该属性值来源前端的序号。</param>
    private readonly record struct TrackedProperty(PropertyModel Model, int Priority, int FrontendIndex);
}
