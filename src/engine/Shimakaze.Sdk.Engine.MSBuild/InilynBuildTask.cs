using System.Text;
using System.Text.Json;

using Microsoft.Build.Framework;

using Shimakaze.Sdk.Inilyn;

using Shimakaze.Sdk.Inilyn.Analyzer.Analysis;
using Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;
using Shimakaze.Sdk.Inilyn.Compilation;

using Task = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Engine.MSBuild;

/// <summary>
/// Inilyn MSBuild 编译与分析任务。
/// </summary>
/// <remarks>
/// <para>
/// 单个任务完成编译与分析两个阶段：接收带 <c>RuleGroup</c> 元数据的 INI 文件
/// 以及 RuleGroup → 输出文件名的映射，按组执行完整编译管线
/// （解析、符号化、语义分析、代码生成），随后可选地对全部编译产物做跨组合法性分析。
/// </para>
/// <para>
/// 使用方式：
/// <code>
/// &lt;Inilyn Include="rules.ini" RuleGroup="rule" /&gt;
/// &lt;InilynRuleGroup Include="rule" OutputFileName="rulesmd.ini" /&gt;
/// </code>
/// </para>
/// </remarks>
public class InilynBuildTask : Task
{
    /// <summary>
    /// 待编译的 INI 文件列表（带 <c>RuleGroup</c> 元数据）。
    /// </summary>
    [Required]
    public ITaskItem[] Files { get; set; } = [];

    /// <summary>
    /// RuleGroup → 输出文件名映射（项标识为组名，<c>OutputFileName</c> 元数据为输出文件名）。
    /// </summary>
    [Required]
    public ITaskItem[] RuleGroups { get; set; } = [];

    /// <summary>
    /// 输出目录。
    /// </summary>
    [Required]
    public string OutputDirectory { get; set; } = ".";

    /// <summary>
    /// 是否启用 TreeShaking（默认 true）。
    /// </summary>
    public bool EnableTreeShaking { get; set; } = true;

    /// <summary>
    /// 分析器规则文件（如 vanilla.xml；多个按声明顺序合并）。
    /// </summary>
    public string[] RuleFiles { get; set; } = [];

    /// <summary>
    /// 外部资源清单文件（如 shp.txt；文件名前缀决定资源种类）。
    /// </summary>
    public string[] AssetFiles { get; set; } = [];

    /// <summary>
    /// 是否对编译产物执行跨组分析（默认 true）。
    /// </summary>
    public bool Analyze { get; set; } = true;

    /// <summary>
    /// 分析错误是否阻断构建（默认 false：错误降级为警告）。
    /// </summary>
    public bool AnalyzeFatal { get; set; }

    /// <summary>
    /// 执行编译与分析。
    /// </summary>
    /// <returns>任务是否成功。</returns>
    public override bool Execute()
    {
        if (Files.Length == 0 || RuleGroups.Length == 0)
        {
            Log.LogMessage(MessageImportance.Low, "没有需要编译的 INI 文件或规则组。");
            return true;
        }

        // 1. 编译：按 RuleGroup 分组
        List<(string Path, string Group)> outputs = [];

        foreach (var group in RuleGroups)
        {
            string groupName = group.ItemSpec;
            string outputFileName = group.GetMetadata("OutputFileName");

            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(outputFileName))
            {
                Log.LogWarning(string.Empty, "INYL003", string.Empty, string.Empty, 0, 0, 0, 0, "跳过缺少名称或输出文件名的 RuleGroup 项。");
                continue;
            }

            List<InilynFile> inputFiles = [];
            foreach (var item in Files)
            {
                if (!string.Equals(item.GetMetadata("RuleGroup"), groupName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string filePath = item.GetMetadata("FullPath");
                if (string.IsNullOrEmpty(filePath))
                {
                    Log.LogWarning(string.Empty, "INYL001", string.Empty, string.Empty, 0, 0, 0, 0, "跳过没有文件路径的 Inilyn 项。");
                    continue;
                }

                if (!File.Exists(filePath))
                {
                    Log.LogError(string.Empty, "INYL002", string.Empty, string.Empty, 0, 0, 0, 0, "文件不存在：{0}", filePath);
                    continue;
                }

                inputFiles.Add(InilynFile.Create(filePath));
            }

            if (inputFiles.Count == 0)
            {
                continue;
            }

            IniCompilationOptions options = new()
            {
                EnableTreeShaking = EnableTreeShaking,
            };

            var result = IniCompilation.Create(inputFiles, options).Emit();
            Log.ReportDiagnostics(result.Diagnostics);

            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            string outputPath = Path.Combine(OutputDirectory, outputFileName);
            StringBuilder merged = new();
            foreach (var kvp in result.OutputFiles)
            {
                merged.AppendLine(kvp.Value);
            }

            // 无 BOM 的 UTF-8，与游戏原生 INI 编码保持一致
            File.WriteAllText(outputPath, merged.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Log.LogMessage(MessageImportance.Normal, "已生成：{0}", outputPath);

            string sourceMapPath = outputPath + ".map.json";
            string sourceMapJson = JsonSerializer.Serialize(result.SourceMap, SourceMapJsonContext.Default.SourceMap);
            File.WriteAllText(sourceMapPath, sourceMapJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Log.LogMessage(MessageImportance.Normal, "已生成 SourceMap：{0}", sourceMapPath);

            outputs.Add((outputPath, groupName));
        }

        // 2. 跨组分析（可选）
        if (Analyze && RuleFiles.Length > 0)
        {
            RunAnalysis(outputs);
        }

        return !Log.HasLoggedErrors;
    }

    private void RunAnalysis(List<(string Path, string Group)> outputs)
    {
        InilynRuleSet ruleSet;
        try
        {
            ruleSet = InilynRuleSet.Load(RuleFiles);
        }
        catch (Exception ex)
        {
            Log.LogError(string.Empty, "INYL501", string.Empty, string.Empty, 0, 0, 0, 0, $"加载规则集失败：{ex.Message}");
            return;
        }

        var assets = LoadAssets();

        List<InilynAnalysisInput> inputs = [];
        foreach (var (path, group) in outputs)
        {
            if (!File.Exists(path))
            {
                Log.LogWarning(string.Empty, "INYL502", string.Empty, string.Empty, 0, 0, 0, 0, $"分析输出不存在：{path}");
                continue;
            }

            inputs.Add(new InilynAnalysisInput(group, path, File.ReadAllText(path)));
        }

        if (inputs.Count == 0)
        {
            Log.LogWarning(string.Empty, "INYL503", string.Empty, string.Empty, 0, 0, 0, 0, "没有可分析的文件。");
            return;
        }

        var analysis = InilynAnalyzer.Analyze(ruleSet, inputs, assets);
        Log.ReportDiagnostics(analysis.Diagnostics, notError: !AnalyzeFatal);

        Log.LogMessage(MessageImportance.Normal, "分析完成：{0} 节，{1} 节可被 TreeShaking 移除。",
            analysis.Sections.Count, analysis.TreeShakeable.Count);
    }

    private Dictionary<string, ISet<string>>? LoadAssets()
    {
        Dictionary<string, ISet<string>>? assets = null;

        foreach (string assetPath in AssetFiles)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || !File.Exists(assetPath))
            {
                continue;
            }

            string kind = Path.GetFileNameWithoutExtension(assetPath).ToUpperInvariant();
            HashSet<string> values = new(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in File.ReadAllLines(assetPath))
            {
                string v = raw.Trim();
                if (v.Length > 0 && v[0] != '#')
                {
                    values.Add(v);
                }
            }

            assets ??= new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase);
            assets[kind] = values;
        }

        return assets;
    }
}
