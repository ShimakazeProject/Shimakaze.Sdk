using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Analyzer.Common;

/// <summary>
/// 分析运行器：按登记顺序执行分析器并合并诊断。
/// </summary>
public static class AnalyzerRunner
{
    private static volatile IAnalyzer[] s_defaultAnalyzers = [];

    /// <summary>
    /// 默认分析器集合；<see cref="AnalyzerOptions.Analyzers"/> 为 <see langword="null"/> 时使用。
    /// </summary>
    /// <remarks>
    /// 具体分析器由所在程序集通过 <see cref="RegisterDefault"/> 登记；框架层不直接引用具体实现。
    /// </remarks>
    public static IReadOnlyList<IAnalyzer> DefaultAnalyzers
        => s_defaultAnalyzers;

    /// <summary>
    /// 登记默认分析器；同名分析器会被替换，其余按登记顺序追加。
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
    /// <returns>编译诊断与分析诊断的合并结果，按源文件与位置排序并去重。</returns>
    public static IReadOnlyList<Diagnostic> Run(
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

        return bag.ToSortedDiagnostics(pathOrder);
    }
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
