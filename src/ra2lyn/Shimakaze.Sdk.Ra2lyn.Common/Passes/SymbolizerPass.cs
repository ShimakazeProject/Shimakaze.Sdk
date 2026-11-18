using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 符号化 Pass：扫描所有 frontend 的原始实体，把每个属性登记进 <see cref="SymbolTable"/>
/// （TODO.md §5.1）。
/// </summary>
/// <remarks>
/// 本 Pass <b>不做</b>合并、求值、分类，只负责"把所有原始实体登记进表"。<br/>
/// 执行位置：<see cref="MergePass"/> 之前。<br/>
/// 登记顺序 = frontend 顺序 = 同一 frontend 内 entity 顺序 = entity 内 property 顺序；
/// 因此同一 <c>(Group, Name, Key)</c> 列表天然按"时间从旧到新"排列，后续 MergePass /
/// ValueResolutionPass 可直接从尾部回溯。
/// <para/>
/// 这里显式遍历 <see cref="CompilationContext.Frontends"/> 而非扁平的
/// <see cref="CompilationContext.Entities"/>，是因为需要记录每条属性所属的 FrontendIndex，
/// 而扁平列表已经丢失了这一来源信息。
/// </remarks>
public sealed class SymbolizerPass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "Symbolizer";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        SymbolTable table = new();

        for (int frontendIndex = 0; frontendIndex < context.Frontends.Count; frontendIndex++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var frontend = context.Frontends[frontendIndex];

            foreach (var entity in frontend.Entities)
            {
                foreach (var property in entity.Properties)
                {
                    table.Add(
                        entity.Group,
                        entity.Name,
                        property.Key,
                        new PropertyEntry(
                            Value: property.Value,
                            Priority: entity.Priority,
                            FrontendIndex: frontendIndex,
                            Origin: property.Origin,
                            IsAppend: false));
                }
            }
        }

        context.SymbolTable = table;
    }
}
