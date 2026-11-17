using Shimakaze.Sdk.Ra2lyn.Common.Passes;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 编译驱动：按固定顺序编排后端 Pass。
/// </summary>
public sealed class Compilation(CompilationOptions options)
{
    private readonly List<FrontendResult> _frontends = [];
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
    /// 依次执行 Pass 并汇总结果。
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
    /// Pass 顺序固定为：合并 → 分类 → 引用 → 发现 → 可达性 → 规则校验。
    /// </summary>
    /// <remarks>
    /// 发现规则会把新发现的实体分类为普通实体，因此规则校验放在最后，保证被发现的实体同样接受类型校验。
    /// </remarks>
    public static IReadOnlyList<ICompilationPass> CreateDefaultPasses()
        =>
        [
            new MergePass(),
            new ClassificationPass(),
            new ReferencePass(),
            new DiscoveryPass(),
            new ReachabilityPass(),
            new RuleValidationPass(),
        ];
}