using Shimakaze.Sdk.Ra2lyn.Analyzer.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common;

/// <summary>
/// 分析器：对一次编译的产物做只读检查，并把诊断写入分析上下文。
/// </summary>
/// <remarks>
/// 分析器与编译 Pass 的分工：Pass 负责构造模型（合并、分类、引用、发现），分析器只负责检查模型。
/// 同一个分析器实例应可在多次运行之间复用，状态一律放在 <see cref="AnalyzerContext"/> 里。
/// </remarks>
public interface IAnalyzer
{
    /// <summary>
    /// 分析器名称，用于诊断来源与运行报告。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 执行分析。
    /// </summary>
    /// <param name="context">分析上下文。</param>
    void Analyze(AnalyzerContext context);
}

/// <summary>
/// 分析上下文：分析阶段的输入模型、规则、语义注解与诊断收集。
/// </summary>
public sealed class AnalyzerContext
{
    /// <summary>
    /// 初始化分析上下文。
    /// </summary>
    /// <param name="model">已经完成 Pass 的编译模型。</param>
    /// <param name="options">分析选项。</param>
    /// <param name="annotations">语义注解；为 <see langword="null"/> 时新建。</param>
    public AnalyzerContext(CompilationModel model, AnalyzerOptions options, SemanticAnnotations? annotations = null)
    {
        Model = model;
        Options = options;
        Annotations = annotations ?? new SemanticAnnotations();
    }

    /// <summary>
    /// 被分析的编译模型；分析器不得改写其中的实体。
    /// </summary>
    public CompilationModel Model { get; }

    /// <summary>
    /// 分析选项。
    /// </summary>
    public AnalyzerOptions Options { get; }

    /// <summary>
    /// 规则集；来自 <see cref="AnalyzerOptions.RuleSet"/>，为 <see langword="null"/> 表示不做规则相关分析。
    /// </summary>
    public RuleSet? Rules
        => Options.RuleSet;

    /// <summary>
    /// 引用图：引用分析器的输出，供可达性分析与发射阶段使用。
    /// </summary>
    public ReferenceGraph Graph { get; } = new();

    /// <summary>
    /// 语义注解：分类、类型与可达性等分析层结论。
    /// </summary>
    public SemanticAnnotations Annotations { get; }

    /// <summary>
    /// 诊断收集器；与编译诊断合并后一起输出。
    /// </summary>
    public DiagnosticBag Diagnostics { get; } = [];

    /// <summary>
    /// 取消令牌。
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// 写入一条诊断；完全相同的诊断会被忽略。
    /// </summary>
    /// <param name="code">诊断代码。</param>
    /// <param name="severity">严重级别。</param>
    /// <param name="message">诊断消息。</param>
    /// <param name="span">诊断位置；没有位置时使用 <see langword="default"/>。</param>
    public void Report(string code, DiagnosticSeverity severity, string message, SourceSpan span = default)
        => Diagnostics.Add(new Diagnostic(code, severity, message, span));
}

/// <summary>
/// 分析选项。
/// </summary>
public class AnalyzerOptions
{
    /// <summary>
    /// 要执行的分析器；为 <see langword="null"/> 时使用 <see cref="AnalyzerRunner.DefaultAnalyzers"/>。
    /// </summary>
    public IReadOnlyList<IAnalyzer>? Analyzers { get; init; }

    /// <summary>
    /// 分析规则集；为 <see langword="null"/> 表示只做与规则无关的分析。
    /// </summary>
    public RuleSet? RuleSet { get; init; }

    /// <summary>
    /// 分析器抛出的未处理异常是否降级为警告；默认关闭，让错误直接暴露。
    /// </summary>
    public bool SuppressAnalyzerExceptions { get; init; }
}
