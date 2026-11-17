using Shimakaze.Sdk.Ra2lyn.Common.Passes;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 编译驱动：按固定顺序编排后端 Pass。
/// </summary>
/// <remarks>
/// 规则校验等“检查模型”的工作已从 Pass 中拆出，由外部分析器通过 <see cref="AddAnalysis"/> 接入；
/// 内核只负责构造模型，不依赖任何具体分析器。
/// </remarks>
public sealed class Compilation(CompilationOptions options)
{
    private readonly List<FrontendResult> _frontends = [];
    private readonly List<Func<CompilationContext, IReadOnlyList<Diagnostic>>> _analyses = [];
    private RuleSet? _rules;

    /// <summary>
    /// 加入一个前端结果；顺序即合并顺序。
    /// </summary>
    public Compilation Add(FrontendResult result)
    {
        _frontends.Add(result);
        return this;
    }

    /// <summary>
    /// 设置规则集；没有规则时只做合并与可达性检查。
    /// </summary>
    public Compilation AddRules(RuleSet rules)
    {
        _rules = rules;
        return this;
    }

    /// <summary>
    /// 追加一个分析阶段：在全部 Pass 结束后、结果汇总前执行。
    /// </summary>
    /// <param name="analysis">分析函数，接收已完成的编译上下文并返回诊断。</param>
    /// <remarks>
    /// 分析阶段按登记顺序执行，且只能读到最终模型，不允许改写实体。
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
        CompilationContext context = new(options, _frontends, _rules) { CancellationToken = cancellationToken };

        foreach (var pass in CreateDefaultPasses())
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
        };

        return new CompilationResult
        {
            Model = model,
            Diagnostics = diagnostics,
        };
    }

    /// <summary>
    /// Pass 顺序固定为：合并 → 分类 → 引用 → 发现 → 可达性。
    /// </summary>
    /// <remarks>
    /// 规则校验由外部分析器在 Pass 之后执行；发现规则会把新发现的实体分类为普通实体，
    /// 因此分析阶段必须排在最后，保证被发现的实体同样接受检查。
    /// </remarks>
    public static IReadOnlyList<ICompilationPass> CreateDefaultPasses()
        =>
        [
            new MergePass(),
            new ClassificationPass(),
            new ReferencePass(),
            new DiscoveryPass(),
            new ReachabilityPass(),
        ];
}
