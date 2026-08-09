using System.Text;

using Shimakaze.Sdk.Inilyn.Analyzer.Analysis;
using Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

namespace Shimakaze.Sdk.Engine.Ini;

/// <summary>
/// INI 编译与分析管线公共工具，供 CLI 与 MSBuild 任务复用。
/// </summary>
public static class IniTool
{
    /// <summary>
    /// 加载外部资源清单文件（如 shp.txt；文件名前缀决定资源种类）。
    /// </summary>
    /// <param name="assetFiles">资源清单文件路径。</param>
    /// <param name="onError">文件缺失时的错误回调；为 null 时静默跳过缺失文件。</param>
    /// <returns>资源种类 → 合法值集合；没有可用清单时返回 <see langword="null"/>。</returns>
    public static Dictionary<string, ISet<string>>? LoadAssets(IEnumerable<string> assetFiles, Action<string>? onError = null)
    {
        Dictionary<string, ISet<string>>? assets = null;
        foreach (string file in assetFiles)
        {
            if (!File.Exists(file))
            {
                onError?.Invoke($"资源清单不存在 - {file}");
                continue;
            }

            string kind = Path.GetFileNameWithoutExtension(file).ToUpperInvariant();
            HashSet<string> values = new(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in File.ReadAllLines(file))
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

    /// <summary>
    /// 对编译产物执行跨组合法性分析。
    /// </summary>
    /// <param name="ruleSet">规则集。</param>
    /// <param name="inputs">编译产物（每个输入 = 一个规则组的一份 INI 内容）。</param>
    /// <param name="assets">可选的外部资源清单（种类 → 合法值集合）。</param>
    /// <returns>分析结果。</returns>
    public static InilynAnalysis Analyze(
        InilynRuleSet ruleSet,
        IEnumerable<InilynAnalysisInput> inputs,
        IReadOnlyDictionary<string, ISet<string>>? assets)
        => InilynAnalyzer.Analyze(ruleSet, inputs, assets);

    /// <summary>
    /// 将多个输出文件的内容合并为一段文本。
    /// </summary>
    /// <param name="outputFiles">输出文件映射（键为文件名，值为生成的 INI 文本）。</param>
    /// <returns>合并后的文本。</returns>
    public static string MergeOutputFiles(IEnumerable<KeyValuePair<string, string>> outputFiles)
    {
        StringBuilder merged = new();
        foreach (var kvp in outputFiles)
        {
            merged.AppendLine(kvp.Value);
        }

        return merged.ToString();
    }
}
