using Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Passes;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common;

/// <summary>
/// 分析运行器：按登记顺序执行分析器并汇总结果。
/// </summary>
public static class AnalyzerRunner
{
    private static volatile IAnalyzer[] s_defaultAnalyzers =
    [
        new ClassificationAnalyzer(),
        new ReferenceAnalyzer(),
        new DiscoveryAnalyzer(),
        new ReachabilityAnalyzer(),
    ];

    /// <summary>
    /// 默认分析器链：分类 → 引用 → 发现 → 可达性；规则校验类分析器由宿主追加。
    /// </summary>
    /// <remarks>
    /// 顺序不可随意调整：引用依赖分类，发现依赖引用，可达性依赖引用图。
    /// </remarks>
    public static IReadOnlyList<IAnalyzer> DefaultAnalyzers
        => s_defaultAnalyzers;

    /// <summary>
    /// 登记默认分析器；同名分析器会被替换，其余按登记顺序追加到链尾。
    /// </summary>
    /// <param name="analyzers">分析器集合。</param>
    public static void RegisterDefault(IEnumerable<IAnalyzer> analyzers)
    {
        List<IAnalyzer> merged = [.. s_defaultAnalyzers];
        foreach (var analyzer in analyzers)
        {
            int index = merged.FindIndex(item => string.Equals(item.Name, analyzer.Name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                merged[index] = analyzer;
            else
                merged.Add(analyzer);
        }

        s_defaultAnalyzers = [.. merged];
    }

    /// <summary>
    /// 对一份已完成的编译模型执行全部分析器。
    /// </summary>
    /// <param name="model">编译模型。</param>
    /// <param name="options">分析选项；为 <see langword="null"/> 时使用默认分析器。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>分析结果：注解、引用图与合并后的诊断。</returns>
    public static AnalyzerResult Run(
        CompilationModel model,
        AnalyzerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new AnalyzerOptions();
        AnalyzerContext context = new(model, options) { CancellationToken = cancellationToken };

        IReadOnlyList<IAnalyzer> analyzers = options.Analyzers ?? DefaultAnalyzers;
        foreach (var analyzer in analyzers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                analyzer.Analyze(context);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (options.SuppressAnalyzerExceptions)
            {
                context.Report(
                    AnalyzerDiagnosticCodes.AnalyzerFailed,
                    DiagnosticSeverity.Warning,
                    $"分析器 '{analyzer.Name}' 执行失败：{exception.Message}");
            }
        }

        Dictionary<string, int> pathOrder = [];
        foreach (var source in model.Sources)
            pathOrder[source.Path] = pathOrder.Count;

        DiagnosticBag bag = [];
        foreach (var diagnostic in model.Diagnostics)
            bag.Add(diagnostic);

        foreach (var diagnostic in context.Diagnostics)
            bag.Add(diagnostic);

        return new()
        {
            Annotations = context.Annotations,
            Graph = context.Graph,
            Diagnostics = bag.ToSortedDiagnostics(pathOrder),
        };
    }
}

/// <summary>
/// 分析结果。
/// </summary>
public sealed class AnalyzerResult
{
    /// <summary>
    /// 语义注解：分类、类型与可达性。
    /// </summary>
    public required SemanticAnnotations Annotations { get; init; }

    /// <summary>
    /// 引用图。
    /// </summary>
    public required ReferenceGraph Graph { get; init; }

    /// <summary>
    /// 编译诊断与分析诊断的合并结果，已排序去重。
    /// </summary>
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }
}

/// <summary>
/// 分析器框架自有的诊断代码。
/// </summary>
public static class AnalyzerDiagnosticCodes
{
    /// <summary>
    /// ANC001：分析器执行过程中抛出未处理异常。
    /// </summary>
    public const string AnalyzerFailed = "ANC001";
}
