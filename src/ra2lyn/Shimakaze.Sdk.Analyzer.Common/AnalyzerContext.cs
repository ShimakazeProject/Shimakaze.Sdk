using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Analyzer.Common;

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
/// 分析上下文：分析阶段的输入模型、规则与诊断收集。
/// </summary>
/// <param name="model">已经完成 Pass 的编译模型。</param>
/// <param name="options">分析选项。</param>
public sealed class AnalyzerContext(CompilationModel model, AnalyzerOptions options)
{
    /// <summary>
    /// 被分析的编译模型；分析器不得改写其中的实体。
    /// </summary>
    public CompilationModel Model { get; } = model;

    /// <summary>
    /// 分析选项。
    /// </summary>
    public AnalyzerOptions Options { get; } = options;

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
    /// 分析器抛出的未处理异常是否降级为警告；默认关闭，让错误直接暴露。
    /// </summary>
    public bool SuppressAnalyzerExceptions { get; init; }
}
