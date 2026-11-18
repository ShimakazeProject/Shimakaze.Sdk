using Shimakaze.Sdk.Ra2lyn.Common.Passes;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 编译驱动：按固定顺序编排后端 Pass，并为外部阶段提供挂载点。
/// </summary>
/// <remarks>
/// 内核只认识“节 / 组 / 键 / 简单值或引用值”。分类、引用、发现、规则校验等具体语义
/// 由外部分析器通过 <see cref="AddPass"/> 与 <see cref="AddAnalysis"/> 接入，内核不依赖任何具体规则。
/// </remarks>
public sealed class Compilation(CompilationOptions options)
{
    private readonly List<FrontendResult> _frontends = [];
    private readonly List<ICompilationPass> _passes = [];
    private readonly List<Func<CompilationContext, IReadOnlyList<Diagnostic>>> _analyses = [];

    /// <summary>
    /// 加入一个前端结果；顺序即合并顺序。
    /// </summary>
    public Compilation Add(FrontendResult result)
    {
        _frontends.Add(result);
        return this;
    }

    /// <summary>
    /// 追加一个 Pass：在内置 Pass 之后、分析阶段之前执行。
    /// </summary>
    /// <param name="pass">Pass 实例。</param>
    public Compilation AddPass(ICompilationPass pass)
    {
        _passes.Add(pass);
        return this;
    }

    /// <summary>
    /// 追加一个分析阶段：在全部 Pass 结束后、结果汇总前执行。
    /// </summary>
    /// <param name="analysis">分析函数，接收已完成的编译上下文并返回诊断。</param>
    /// <remarks>
    /// 分析阶段按登记顺序执行，只能读取最终模型，不允许改写实体。
    /// </remarks>
    public Compilation AddAnalysis(Func<CompilationContext, IReadOnlyList<Diagnostic>> analysis)
    {
        _analyses.Add(analysis);
        return this;
    }

    /// <summary>
    /// 依次执行 Pass 与分析，并汇总结果。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public CompilationResult Run(CancellationToken cancellationToken = default)
    {
        CompilationContext context = new(options, _frontends) { CancellationToken = cancellationToken };

        IReadOnlyList<ICompilationPass> passes = [.. CreateDefaultPasses(), .. _passes];
        foreach (var pass in passes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pass.Run(context);
        }

        foreach (var analysis in _analyses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var diagnostic in analysis(context))
                context.Diagnostics.Add(diagnostic);
        }

        Dictionary<string, int> pathOrder = [];
        foreach (var source in context.Sources)
            pathOrder[source.Path] = pathOrder.Count;

        List<Diagnostic> diagnostics = [.. context.Diagnostics.ToSortedDiagnostics(pathOrder)];

        CompilationModel model = new()
        {
            Sources = context.Sources,
            Entities = context.Entities,
            References = context.References,
            Diagnostics = diagnostics,
            ReachableEntities = context.ReachableEntities.Count is 0 ? null : context.ReachableEntities,
        };

        return new CompilationResult
        {
            Model = model,
            Diagnostics = diagnostics,
        };
    }

    /// <summary>
    /// 内核自带的 Pass：先建立符号表（<see cref="SymbolizerPass"/>），再按优先级合并
    /// （<see cref="MergePass"/>），然后展开模板（<see cref="TemplateExpansionPass"/>），
    /// 最后做值替换（<see cref="ValueResolutionPass"/>，TODO.md §5）。
    /// 其余全部由外部注入。
    /// </summary>
    /// <remarks>
    /// 展开模板必须排在合并之后：模板节也要参与 MergePass（TODO.md §4.5），同名模板先合并再应用一次。
    /// 值替换必须排在模板展开之后：模板 body 注入的 <c>$</c> 占位符由它统一解析（TODO.md §4.4）。
    /// NullCleanupPass（剔除值为 <c>$null</c> 的 key）与基础分析器/额外规则分析器本步骤尚未实现，
    /// 由外部按顺序 <see cref="AddPass"/> 接入。
    /// </remarks>
    public static IReadOnlyList<ICompilationPass> CreateDefaultPasses()
        =>
        [
            new SymbolizerPass(),
            new MergePass(),
            new TemplateExpansionPass(),
            new ValueResolutionPass(),
        ];
}
