using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Common.Template;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 模板展开 Pass：对每个模板评估 <c>where</c> 表达式，把模板 body 的属性应用到匹配的节，
/// 然后把模板节自身从实体列表与符号表中剔除（TODO.md §4.4、§4.5）。
/// </summary>
/// <remarks>
/// <para>
/// 执行位置：<see cref="MergePass"/> 之后、ValueResolutionPass 之前。此时模板节已经参与过合并
/// （§4.5：模板节参与 MergePass），因此同名模板跨文件先合并成一个模板，再统一应用一次
/// （模板的模板不允许，§4.5）。
/// </para>
/// <para>
/// 匹配规则：<c>where</c> 为 <see langword="null"/> 的无条件模板对当前 Group 内所有非模板节生效
/// （§4.2）；有表达式时逐节求值，求值期诊断按表达式偏移换算回模板源文件位置。
/// </para>
/// <para>
/// 注入规则：模板属性的 priority = 目标 entity 的 priority + 1（§4.5），因此总是覆盖目标自己写的值；
/// 被覆盖的值进目标属性的 History 链。模板属性的 <see cref="PropertyModel.Origin"/>（以及 History 里
/// 旧值的 Origin）原样保留，指向模板源文件里的位置，后续分析器在该属性上报错时能指回模板处。
/// </para>
/// <para>
/// 模板 body 里的 <c>$</c> 占位符本步骤不解析，原样保留，由 ValueResolutionPass 统一处理（§4.4）。
/// </para>
/// </remarks>
public sealed class TemplateExpansionPass : ICompilationPass
{
    private readonly WhereExpressionEvaluator _evaluator;

    /// <summary>
    /// 使用默认 <c>where</c> 求值器初始化 Pass。
    /// </summary>
    public TemplateExpansionPass()
        : this(new WhereExpressionEvaluator())
    {
    }

    /// <summary>
    /// 使用指定的 <c>where</c> 求值器初始化 Pass。
    /// </summary>
    /// <param name="evaluator">
    /// 求值器；可携带 <see cref="WhereExpressionEvaluator.TypeMatcher"/>，供后续把基础分析器的类型信息接入。
    /// </param>
    public TemplateExpansionPass(WhereExpressionEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(evaluator);
        _evaluator = evaluator;
    }

    /// <inheritdoc/>
    public string Name => "TemplateExpansion";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        SymbolTable? symbolTable = context.SymbolTable;
        if (symbolTable is null)
            return;

        List<EntityModel> templates =
        [
            .. context.Entities.Where(static entity => entity.IsTemplate),
        ];
        if (templates.Count is 0)
            return;

        // 目标节保持原始顺序；模板节不参与匹配（模板的模板不允许，TODO.md §4.5）。
        List<EntityModel> targets =
        [
            .. context.Entities.Where(static entity => entity.IsTemplate is false),
        ];

        // 每个目标节的展开结果；按 targets 的原始下标存放，供最后按引用回填使用。
        List<EntityModel> applied = [.. targets];

        // 目标节 → 其在 targets 中的下标；键用实体引用（EntityModel 没有重写相等语义）。
        // 回填时按引用查这个索引，不能用 EntityId：EntityId 只在单个前端内唯一（见下方回填处的说明）。
        Dictionary<EntityModel, int> targetIndex = [];
        for (int index = 0; index < targets.Count; index++)
            targetIndex[targets[index]] = index;

        // 先把所有模板应用到所有目标节，再统一剔除模板节：处理期间模板节仍留在
        // context.Entities 里，便于诊断定位与后续排查。
        foreach (var template in templates)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            for (int index = 0; index < targets.Count; index++)
            {
                var target = targets[index];
                if (string.Equals(target.Group, template.Group, StringComparison.OrdinalIgnoreCase) is false)
                    continue;

                WhereEvaluationResult evaluation = Evaluate(template, target, symbolTable);
                foreach (var diagnostic in evaluation.Diagnostics)
                    context.Diagnostics.Add(MakeDiagnostic(template, target, diagnostic));

                if (evaluation.Succeeded is false || evaluation.Matches is false)
                    continue;

                applied[index] = ApplyTemplate(template, applied[index]);
            }
        }

        // 剔除模板节（TODO.md §4.5）：不进入 Emit，也不参与后续分析器。
        symbolTable.Prune(templates.Select(static template => (template.Group, template.Name)));

        // 按原始顺序回填展开后的目标节；模板节整体消失，其它节的顺序与 Id 不变。
        //
        // 这里必须按引用回填，不能按 EntityId 建字典：每个前端各自持有一个 IrBuilder
        // （见 IrBuilder 的说明），EntityId 只在单个前端的源文本内唯一，跨前端会重号；
        // MergePass 的合并结果又继承了"桶内首个 entity"的 Id，因此不同名称的节完全可能共享
        // 同一个 EntityId。按 Id 回填会让后一个节覆盖前一个节，导致节被吞掉、重复并串味。
        List<EntityModel> result = [];
        foreach (var entity in context.Entities)
        {
            if (entity.IsTemplate)
                continue;

            if (targetIndex.TryGetValue(entity, out int index))
                result.Add(applied[index]);
        }

        context.SetEntities(result);
    }

    /// <summary>
    /// 对一个目标节求值模板条件；无条件模板对当前 Group 内所有节生效（TODO.md §4.2）。
    /// </summary>
    private WhereEvaluationResult Evaluate(EntityModel template, EntityModel target, SymbolTable symbolTable)
        => template.TemplateWhere is null
            ? WhereEvaluationResult.MatchesResult(true, [])
            : _evaluator.Evaluate(template.TemplateWhere, target, symbolTable);

    /// <summary>
    /// 把模板 body 的属性应用到一个目标节：同名 key 覆盖，新 key 追加（TODO.md §4.4）。
    /// </summary>
    /// <remarks>
    /// 注入的属性保留模板源位置（诊断回指模板）与来源模板名（<see cref="PropertyModel.TemplateName"/>，
    /// TODO.md §4.5）；被覆盖的旧值进目标属性的 History 链。
    /// </remarks>
    private static EntityModel ApplyTemplate(EntityModel template, EntityModel target)
    {
        List<PropertyModel> properties = [.. target.Properties];

        foreach (var injected in template.Properties)
        {
            int index = properties.FindIndex(item =>
                string.Equals(item.Key, injected.Key, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                // 新 key：Origin 直接指向模板里的位置（TODO.md §4.5 行号保留）。
                properties.Add(injected);
                continue;
            }

            // 覆盖：旧值挂进 History 链，新值保留模板里的 Origin（诊断因此指回模板原位置）。
            var existing = properties[index];
            List<PropertyHistoryEntry> history = [.. existing.History];
            history.Add(CreateHistoryEntry(target, existing));
            properties[index] = new PropertyModel
            {
                Id = injected.Id,
                Key = injected.Key,
                Value = injected.Value,
                ValueKind = injected.ValueKind,
                ListSeparator = injected.ListSeparator,
                Origin = injected.Origin,
                History = history,
                TemplateName = injected.TemplateName,
            };
        }

        return new EntityModel
        {
            Id = target.Id,
            Group = target.Group,
            Name = target.Name,
            BaseNames = target.BaseNames,
            Properties = properties,
            Origin = target.Origin,
            Priority = target.Priority,
            // 目标节不是模板；显式透传以保持模型字段完整（模板节本身由本 Pass 整体剔除）。
            IsTemplate = target.IsTemplate,
            TemplateWhere = target.TemplateWhere,
            TemplateWhereSpan = target.TemplateWhereSpan,
        };
    }

    /// <summary>
    /// 构造一条"被模板覆盖掉"的历史记录。
    /// </summary>
    /// <remarks>
    /// 模板属性的 priority = 目标 entity 的 priority + 1（TODO.md §4.5），所以旧值（也就是目标节自己写下的值）
    /// 的 priority 记为目标 entity 的 priority。FrontendIndex 取现有 History 链里的来源（链为空时无法回溯
    /// 该值所在前端，记 0）；该字段只用于符号表回溯排序，模板注入发生在剔除之后，不依赖它做精确排序。
    /// </remarks>
    private static PropertyHistoryEntry CreateHistoryEntry(EntityModel target, PropertyModel existing)
    {
        int frontendIndex = existing.History.Count is 0 ? 0 : existing.History[^1].FrontendIndex;
        return new PropertyHistoryEntry(
            existing.Value,
            existing.Origin,
            target.Priority,
            frontendIndex,
            IsAppend: false);
    }

    /// <summary>
    /// 把求值诊断换算回模板源文件里的位置。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="WhereDiagnostic.Offset"/> / <see cref="WhereDiagnostic.Length"/> 的语义是
    /// <b>相对 <c>where</c> 表达式文本</b>的偏移（见 <see cref="WhereExpressionEvaluator"/>：节点位置直接取自
    /// <c>WhereExpressionNode.Start/End</c>）。表达式在源文件里的真实起点由
    /// <see cref="EntityModel.TemplateWhereSpan"/> 给出（前端从 <c>TemplateNode.WhereSpan</c> 降级而来，
    /// 已裁剪首尾空白），因此源位置 = 表达式起点 + 表达式内偏移，直接相加即可。
    /// </para>
    /// <para>
    /// 这里不能用模板实体 <see cref="EntityModel.Origin"/> 倒推：它覆盖整个节头、且会随 MergePass
    /// 取"桶内首个 entity"的位置；倒推时假设诊断片段贴着节头末尾，对 <c>A &amp;&amp; B</c> 中间位置的节点会算错。
    /// </para>
    /// <para>
    /// 只有在拿不到表达式位置时（例如手工构造的 <see cref="EntityModel"/> 没有填
    /// <see cref="EntityModel.TemplateWhereSpan"/>）才退回粗略的节头区间。
    /// </para>
    /// </remarks>
    private static Diagnostic MakeDiagnostic(EntityModel template, EntityModel target, WhereDiagnostic diagnostic)
    {
        SourceSpan origin = template.Origin;
        SourceSpan span;
        if (template.TemplateWhereSpan.IsEmpty)
        {
            // 没有表达式位置：退回模板节头区间，至少把读者指到正确的节。
            span = origin;
        }
        else
        {
            SourceSpan expression = template.TemplateWhereSpan;
            int start = Math.Clamp(expression.Start + diagnostic.Offset, expression.Start, expression.End);
            int end = Math.Clamp(start + Math.Max(1, diagnostic.Length), start, expression.End);
            span = new(expression.Path, start, end);
        }

        string message = diagnostic.Kind is WhereDiagnosticKind.NullReference
            ? $"模板 '{template.Name}' 应用于节 '{target.Name}' 时求值失败: {diagnostic.Message}"
            : diagnostic.Message;

        return new Diagnostic(diagnostic.Code, DiagnosticSeverity.Error, message, span);
    }
}
