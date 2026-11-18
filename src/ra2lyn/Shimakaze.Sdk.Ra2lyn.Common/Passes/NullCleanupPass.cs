using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 空值清理 Pass：把 <see cref="ValueResolutionPass"/> 求值为 <c>$null</c>（即 <see cref="PropertyModel.Value"/>
/// 为 <see langword="null"/>）的属性从最终 entity 的 <see cref="EntityModel.Properties"/> 列表中剔除
/// （TODO.md §3.5.8、§5）。
/// </summary>
/// <remarks>
/// <para>
/// 执行位置：<see cref="ValueResolutionPass"/> 之后、分析阶段之前（TODO.md §5）。
/// 此时所有 <c>$</c> 引用、<c>$</c> 占位符与 <c>$null</c> 常量都已求值完毕，
/// 分析器随后看到的就是最终真实值（已剔除 <c>$null</c> key）。
/// </para>
/// <para>
/// 识别规则：<see cref="PropertyModel.Value"/> 被步骤 6 以 <c>null!</c> 赋值表示"值为 null"
/// （<see langword="required"/> 注解仅约束静态写法，运行时可能为 <see langword="null"/>）。
/// 本 Pass 用 <c>is null</c> 判断，命中即从列表剔除。
/// </para>
/// <para>
/// History 语义（TODO.md §3.5.8）：被 <c>$null</c> 删除的 key 自身的 History 链随 PropertyModel
/// 一并从列表删除；"被 <c>$null</c> 删除的 key 仍出现在 History 链中"指的是——后续以同名 key 覆盖回来
/// 的属性，其 History 链里仍然能看到该 key 历史上被覆盖掉的旧值（包括被 <c>$null</c> 删除前的那个旧值），
/// 由 MergePass / TemplateExpansionPass 在合并时写入，本 Pass 不改动。
/// </para>
/// <para>
/// 空 entity：即使一个 entity 的所有属性都被剔除，entity 本身仍保留在 <see cref="CompilationContext.Entities"/>
/// 列表里（它仍然是源文件中的一个节）；是否最终发射由后续 TreeShaking / 分析器决定，不在本 Pass 范围内。
/// </para>
/// </remarks>
public sealed class NullCleanupPass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "NullCleanup";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Entities.Count is 0)
            return;

        List<EntityModel> cleaned = [];
        foreach (var entity in context.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            // 模板节在 TemplateExpansionPass 后已整体剔除（§4.5）；
            // 即便仍有残留（异常路径），也按与 ValueResolutionPass 一致的方式原样带走。
            if (entity.IsTemplate)
            {
                cleaned.Add(entity);
                continue;
            }

            // 过滤掉值为 null 的属性；其余属性、History 链与顺序原样保留。
            List<PropertyModel> properties =
            [
                .. entity.Properties.Where(static p => p.Value is not null),
            ];

            cleaned.Add(new EntityModel
            {
                Id = entity.Id,
                Group = entity.Group,
                Name = entity.Name,
                BaseNames = entity.BaseNames,
                Properties = properties,
                Origin = entity.Origin,
                Priority = entity.Priority,
                IsTemplate = entity.IsTemplate,
                TemplateWhere = entity.TemplateWhere,
                TemplateWhereSpan = entity.TemplateWhereSpan,
            });
        }

        context.SetEntities(cleaned);
    }
}
