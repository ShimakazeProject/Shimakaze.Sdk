using System.Globalization;
using System.Text;

using Shimakaze.Sdk.Analyzer.Common;
using Shimakaze.Sdk.Analyzer.Common.Rules;
using Shimakaze.Sdk.Analyzer.Common.Types;
using Shimakaze.Sdk.Analyzer.IniRule;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Emit;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Ini;
using Shimakaze.Sdk.Ra2lyn.Ini.Emit;

namespace Shimakaze.Sdk.Engine.Ra2lyn;

/// <summary>
/// Ra2lyn 编译入口：只调用前端门面、编译驱动与分析器，不直接操作 IR 内部字段。
/// </summary>
/// <remarks>
/// 输入按扩展名归入分组：扩展名本身即组名（例如 <c>rulesmd.ini</c> 归入 <c>ini</c> 组）；
/// 需要自定义分组时由调用方通过 <see cref="Ra2lynCompileOptions.Group"/> 显式指定。
/// 语义分析（分类、引用、发现、可达性、规则校验）全部交给分析器链完成。
/// </remarks>
public sealed class Ra2lynCompiler
{
    // 规则分析器由宿主登记；分析器框架本身不引用任何具体实现。
    static Ra2lynCompiler()
    {
        IniRuleAnalyzerRegistration.Register();
    }

    private readonly IniFrontend _frontend = new();

    /// <summary>
    /// 编译一组 INI 文件并把产物写入输出目录。
    /// </summary>
    /// <param name="sources">源文件，顺序即跨文件合并顺序。</param>
    /// <param name="ruleFiles">规则 XML 文件，按顺序合并。</param>
    /// <param name="outputDirectory">输出目录。</param>
    /// <param name="options">编译选项。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>编译与发射产生的全部诊断。</returns>
    public async Task<Ra2lynCompileResult> CompileAsync(
        IReadOnlyList<FileInfo> sources,
        IReadOnlyList<FileInfo> ruleFiles,
        DirectoryInfo outputDirectory,
        Ra2lynCompileOptions options,
        CancellationToken cancellationToken = default)
    {
        List<SourceFile> sourceFiles = [];
        Compilation compilation = new(new CompilationOptions());

        foreach (var source in sources)
        {
            string text = await File.ReadAllTextAsync(source.FullName, Encoding.UTF8, cancellationToken);
            SourceFile file = new(source.FullName, text);
            sourceFiles.Add(file);

            string group = options.Group ?? GetGroupName(source);
            compilation.Add(_frontend.Parse(file, group, new IniParseOptions { Defines = options.Defines }));
        }

        RuleSet? rules = null;
        if (ruleFiles.Count is not 0)
        {
            string[] paths = [.. ruleFiles.Select(static file => file.FullName)];
            rules = await RuleSetReader.ReadFilesAsync(paths, cancellationToken);
        }

        // 分析器链在内核完成合并后进行：先分类，再引用/发现，最后可达性与规则校验。
        compilation.AddAnalysis(context => RunAnalyzers(context, rules, options));

        CompilationResult compilationResult = compilation.Run(cancellationToken);
        IReadOnlyList<Diagnostic> diagnostics = compilationResult.Diagnostics;
        if (diagnostics.Any(static diagnostic => diagnostic.IsError))
            return new() { Diagnostics = diagnostics, Sources = sourceFiles, FilePaths = [] };

        CompilationModel model = new()
        {
            Sources = compilationResult.Model.Sources,
            Entities = compilationResult.Model.Entities,
            References = compilationResult.Model.References,
            Diagnostics = diagnostics,
            ReachableEntities = compilationResult.Model.ReachableEntities,
        };

        EmitResult emitResult = new IniWriter().Emit(
            model,
            new EmitOptions { TreeShaking = options.TreeShaking, NewLine = options.NewLine });

        outputDirectory.Create();
        List<string> writtenPaths = [];
        foreach ((string fileName, string content) in emitResult.Files)
        {
            string path = Path.Combine(outputDirectory.FullName, fileName);
            await File.WriteAllTextAsync(path, content, Encoding.UTF8, cancellationToken);
            writtenPaths.Add(path);
        }

        return new()
        {
            Diagnostics = [.. diagnostics, .. emitResult.Diagnostics],
            Sources = sourceFiles,
            FilePaths = writtenPaths,
        };
    }

    /// <summary>
    /// 运行分析器链，并把引用图与可达性结论写回编译上下文。
    /// </summary>
    private static IReadOnlyList<Diagnostic> RunAnalyzers(CompilationContext context, RuleSet? rules, Ra2lynCompileOptions options)
    {
        CompilationModel model = new()
        {
            Sources = context.Sources,
            Entities = context.Entities,
            References = context.References,
            Diagnostics = [],
        };

        AnalyzerResult result = AnalyzerRunner.Run(
            model,
            new IniRuleAnalyzerOptions { RuleSet = rules, ValidationMode = options.ValidationMode },
            context.CancellationToken);

        context.SetReferences(result.Graph.References);
        context.ApplyReachability(
            context.Entities
                .Where(entity => result.Annotations.IsReachable(entity.Id))
                .Select(static entity => entity.Id));
        return result.Diagnostics;
    }

    /// <summary>
    /// 把诊断格式化为 <c>路径(line, col): 严重级别 CODE: 消息</c> 形式的文本。
    /// </summary>
    /// <param name="result">编译结果。</param>
    public static string FormatDiagnostics(Ra2lynCompileResult result)
    {
        StringBuilder builder = new();
        foreach (var diagnostic in result.Diagnostics)
        {
            string location = string.Empty;
            if (diagnostic.Span.Path is { } path)
            {
                SourceFile? source = result.Sources.FirstOrDefault(item => string.Equals(item.Path, path, StringComparison.Ordinal));
                if (source is null)
                {
                    location = path;
                }
                else
                {
                    (int line, int column) = SourceLocator.GetPosition(source, diagnostic.Span.Start);
                    location = string.Create(CultureInfo.InvariantCulture, $"{path}({line + 1},{column + 1})");
                }
            }

            builder
                .Append(location)
                .Append(location.Length is 0 ? string.Empty : ": ")
                .Append(diagnostic.Severity)
                .Append(' ')
                .Append(diagnostic.Code)
                .Append(": ")
                .Append(diagnostic.Message)
                .AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static string GetGroupName(FileInfo source)
        => Path.GetExtension(source.Name).TrimStart('.').ToLowerInvariant();
}

/// <summary>
/// Ra2lyn 编译结果。
/// </summary>
public sealed record class Ra2lynCompileResult
{
    /// <summary>
    /// 编译与发射诊断。
    /// </summary>
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }

    /// <summary>
    /// 参与编译的源文件，用于计算诊断行列。
    /// </summary>
    public required IReadOnlyList<SourceFile> Sources { get; init; }

    /// <summary>
    /// 实际写出的文件路径。
    /// </summary>
    public required IReadOnlyList<string> FilePaths { get; init; }

    /// <summary>
    /// 是否存在错误级诊断。
    /// </summary>
    public bool HasError
        => Diagnostics.Any(static diagnostic => diagnostic.IsError);
}

/// <summary>
/// Ra2lyn 编译选项。
/// </summary>
public sealed record class Ra2lynCompileOptions
{
    /// <summary>
    /// 解析时使用的分组；为 <see langword="null"/> 时按扩展名推断。
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// 预处理器宏：列出的名字在条件指令中为真，未列出的为假。
    /// </summary>
    public IReadOnlyList<string> Defines { get; init; } = [];

    /// <summary>
    /// 是否在输出时丢弃不可达实体。
    /// </summary>
    public bool TreeShaking { get; init; }

    /// <summary>
    /// 规则分析器使用的值校验模式。
    /// </summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.Loose;

    /// <summary>
    /// 输出换行符。
    /// </summary>
    public string NewLine { get; init; } = "\r\n";
}
