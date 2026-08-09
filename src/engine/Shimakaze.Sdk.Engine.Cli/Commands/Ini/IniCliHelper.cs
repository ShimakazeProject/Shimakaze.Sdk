using Shimakaze.Sdk.Engine.Ini;
using Shimakaze.Sdk.Inilyn.Analyzer.Analysis;
using Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;
using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Ini;

internal static class IniCliHelper
{
    public static InilynRuleSet? LoadRuleSet(IEnumerable<FileInfo> ruleFiles)
    {
        List<string> paths = [];
        foreach (var file in ruleFiles)
        {
            if (!file.Exists)
            {
                Console.Error.WriteLine($"错误：规则文件不存在 - {file.FullName}");
                continue;
            }

            paths.Add(file.FullName);
        }

        if (paths.Count == 0)
        {
            Console.Error.WriteLine("错误：没有可用的规则文件。");
            return null;
        }

        return InilynRuleSet.Load(paths);
    }

    public static Dictionary<string, ISet<string>>? LoadAssets(IEnumerable<FileInfo> assetFiles)
        => IniTool.LoadAssets(assetFiles.Select(static f => f.FullName), msg => Console.Error.WriteLine($"错误：{msg}"));

    public static bool RunAnalysis(
        InilynRuleSet ruleSet,
        IEnumerable<InilynAnalysisInput> inputs,
        IReadOnlyDictionary<string, ISet<string>>? assets,
        bool verbose)
    {
        var analysis = IniTool.Analyze(ruleSet, inputs, assets);
        ReportDiagnostics(analysis.Diagnostics, verbose);

        if (verbose)
        {
            Console.WriteLine($"分析统计：共 {analysis.Sections.Count} 节，{analysis.TreeShakeable.Count} 节可被 TreeShaking 移除。");
        }

        return analysis.HasErrors;
    }

    public static void ReportDiagnostics(IReadOnlyList<Diagnostic> diagnostics, bool verbose)
    {
        foreach (var diagnostic in diagnostics)
        {
            string location = diagnostic.FilePath is not null
                ? diagnostic.EndLine > 0
                    ? $"{diagnostic.FilePath}({diagnostic.Line}, {diagnostic.Column})-({diagnostic.EndLine}, {diagnostic.EndColumn})"
                    : $"{diagnostic.FilePath}({diagnostic.Line}, {diagnostic.Column})"
                : diagnostic.EndLine > 0
                    ? $"(行 {diagnostic.Line}, 列 {diagnostic.Column})-({diagnostic.EndLine}, {diagnostic.EndColumn})"
                    : $"(行 {diagnostic.Line}, 列 {diagnostic.Column})";

            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Error:
                    Console.Error.WriteLine($"错误 {diagnostic.Code}: {diagnostic.Message} {location}");
                    break;
                case DiagnosticSeverity.Warning:
                    Console.Error.WriteLine($"警告 {diagnostic.Code}: {diagnostic.Message} {location}");
                    break;
                case DiagnosticSeverity.Info when verbose:
                    Console.WriteLine($"信息 {diagnostic.Code}: {diagnostic.Message} {location}");
                    break;
            }
        }
    }
}
