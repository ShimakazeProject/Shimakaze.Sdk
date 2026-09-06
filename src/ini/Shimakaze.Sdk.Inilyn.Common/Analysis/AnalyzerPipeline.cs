using Shimakaze.Sdk.Inilyn.Analysis.Lexer;
using Shimakaze.Sdk.Inilyn.Analysis.Parser;
using Shimakaze.Sdk.Inilyn.Analysis.Semantic;
using Shimakaze.Sdk.Inilyn.Data.Lexer;
using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Analysis;

/// <summary>
/// 分析器管道，按顺序编排词法、语法和语义分析器。
/// </summary>
public sealed class AnalyzerPipeline
{
    private ILexerAnalyzer? _lexer;
    private IParserAnalyzer? _parser;
    private ISemanticAnalyzer? _semantic;

    /// <summary>
    /// 注册词法分析器。
    /// </summary>
    /// <param name="lexer">词法分析器实例。</param>
    /// <returns>当前管道实例（支持链式调用）。</returns>
    public AnalyzerPipeline AddLexer(ILexerAnalyzer lexer)
    {
        _lexer = lexer;
        return this;
    }

    /// <summary>
    /// 注册语法分析器。
    /// </summary>
    /// <param name="parser">语法分析器实例。</param>
    /// <returns>当前管道实例（支持链式调用）。</returns>
    public AnalyzerPipeline AddParser(IParserAnalyzer parser)
    {
        _parser = parser;
        return this;
    }

    /// <summary>
    /// 注册语义分析器。
    /// </summary>
    /// <param name="semantic">语义分析器实例。</param>
    /// <returns>当前管道实例（支持链式调用）。</returns>
    public AnalyzerPipeline AddSemantic(ISemanticAnalyzer semantic)
    {
        _semantic = semantic;
        return this;
    }

    /// <summary>
    /// 执行分析管道。
    /// </summary>
    /// <param name="source">源文本。</param>
    /// <param name="documentId">文档 ID。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>管道运行结果，包含记号、语法节点、语义分析结果和诊断信息。</returns>
    public AnalyzerPipelineResult Run(string source, Guid documentId, CancellationToken cancellationToken = default)
    {
        var context = new AnalyzerContext(source, documentId, cancellationToken);
        var diagnostics = new List<AnalyzerDiagnostic>();

        // 词法分析
        IReadOnlyList<IniToken> tokens = [];
        if (_lexer is not null)
        {
            try
            {
                tokens = _lexer.Lex(context);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                diagnostics.Add(new AnalyzerDiagnostic(
                    "INIL001",
                    $"词法分析失败: {ex.Message}",
                    AnalyzerDiagnosticSeverity.Error,
                    default));
            }
        }

        // 语法分析
        IReadOnlyList<SyntaxNode> nodes = [];
        if (_parser is not null && tokens.Count > 0)
        {
            try
            {
                nodes = _parser.Parse(context, tokens);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                diagnostics.Add(new AnalyzerDiagnostic(
                    "INIP001",
                    $"语法分析失败: {ex.Message}",
                    AnalyzerDiagnosticSeverity.Error,
                    default));
            }
        }

        // 语义分析
        SemanticAnalysisResult? semanticResult = null;
        if (_semantic is not null && nodes.Count > 0)
        {
            try
            {
                semanticResult = _semantic.Analyze(context, nodes);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                diagnostics.Add(new AnalyzerDiagnostic(
                    "INIS001",
                    $"语义分析失败: {ex.Message}",
                    AnalyzerDiagnosticSeverity.Error,
                    default));
            }
        }

        return new AnalyzerPipelineResult
        {
            Tokens = tokens,
            Nodes = nodes,
            SemanticResult = semanticResult,
            Diagnostics = diagnostics,
        };
    }
}

/// <summary>
/// 分析器管道的完整输出结果。
/// </summary>
public sealed class AnalyzerPipelineResult
{
    /// <summary>
    /// 词法分析输出的记号列表。
    /// </summary>
    public IReadOnlyList<IniToken> Tokens { get; init; } = [];

    /// <summary>
    /// 语法分析输出的语法节点列表。
    /// </summary>
    public IReadOnlyList<SyntaxNode> Nodes { get; init; } = [];

    /// <summary>
    /// 语义分析结果，可能为 <see langword="null"/>（未注册语义分析器或语法分析失败时）。
    /// </summary>
    public SemanticAnalysisResult? SemanticResult { get; init; }

    /// <summary>
    /// 分析过程中产生的诊断信息。
    /// </summary>
    public List<AnalyzerDiagnostic> Diagnostics { get; init; } = [];

    /// <summary>
    /// 分析是否成功（无错误诊断）。
    /// </summary>
    public bool IsSuccess => Diagnostics.TrueForAll(d => d.Severity != AnalyzerDiagnosticSeverity.Error);
}
