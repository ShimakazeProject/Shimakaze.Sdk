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
    {
        Dictionary<string, ISet<string>>? assets = null;
        foreach (var file in assetFiles)
        {
            if (!file.Exists)
            {
                Console.Error.WriteLine($"错误：资源清单不存在 - {file.FullName}");
                continue;
            }

            string kind = Path.GetFileNameWithoutExtension(file.Name).ToUpperInvariant();
            HashSet<string> values = new(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in File.ReadAllLines(file.FullName))
            {
                string v = raw.Trim();
                if (v.Length > 0 && !v.StartsWith('#'))
                {
                    values.Add(v);
                }
            }

            assets ??= new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase);
            assets[kind] = values;
        }

        return assets;
    }

    public static bool RunAnalysis(
        InilynRuleSet ruleSet,
        IEnumerable<InilynAnalysisInput> inputs,
        IReadOnlyDictionary<string, ISet<string>>? assets,
        bool verbose)
    {
        var analysis = InilynAnalyzer.Analyze(ruleSet, inputs, assets);
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
