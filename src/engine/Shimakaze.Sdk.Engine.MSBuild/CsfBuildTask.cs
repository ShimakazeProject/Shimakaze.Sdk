using Microsoft.Build.Framework;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;
using Shimakaze.Sdk.Engine.Csf;

using Task = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Engine.MSBuild;

/// <summary>
/// Csf MSBuild 编译与合并任务。
/// </summary>
/// <remarks>
/// <para>
/// 接收带 <c>Format</c> 元数据的 <c>CsfResource</c> 源文件（<c>Csf</c> 二进制、
/// <c>YamlV1</c>、<c>XmlV1</c>、<c>JsonV1</c>、<c>JsonV2</c>），逐个编译为 CSF
/// 二进制并写入中间目录，随后按声明顺序合并为一个最终的 CSF 文件。
/// </para>
/// <para>
/// 使用方式：
/// <code>
/// &lt;CsfResource Include="base.csf" Format="Csf" /&gt;
/// &lt;CsfResource Include="strings.csf.yaml" Format="YamlV1" /&gt;
/// &lt;CsfResource Include="extra.csf.xml" Format="XmlV1" /&gt;
/// &lt;CsfResource Include="patch.csf.json" Format="JsonV1" /&gt;
/// </code>
/// </para>
/// </remarks>
public class CsfBuildTask : Task
{
    /// <summary>
    /// 待编译的 CSF 源文件列表（带 <c>Format</c> 元数据）。
    /// </summary>
    [Required]
    public ITaskItem[] Sources { get; set; } = [];

    /// <summary>
    /// 编译中间文件输出目录（通常位于 obj 下）。
    /// </summary>
    [Required]
    public string IntermediateDirectory { get; set; } = "obj\\csf";

    /// <summary>
    /// 合并后的最终 CSF 文件路径。
    /// </summary>
    [Required]
    public string OutputFile { get; set; } = "";

    /// <summary>
    /// 项目目录，用于计算源文件的相对中间路径。
    /// </summary>
    public string ProjectDirectory { get; set; } = "";

    /// <summary>
    /// 执行编译与合并。
    /// </summary>
    /// <returns>任务是否成功。</returns>
    public override bool Execute()
    {
        if (Sources.Length == 0)
        {
            Log.LogMessage(MessageImportance.Low, "没有需要编译的 CSF 文件。");
            return true;
        }

        Directory.CreateDirectory(IntermediateDirectory);

        List<CsfData> compiled = [];
        foreach (ITaskItem item in Sources)
        {
            string filePath = item.GetMetadata("FullPath");
            if (string.IsNullOrEmpty(filePath))
            {
                Log.LogWarning(string.Empty, "CSF001", string.Empty, string.Empty, 0, 0, 0, 0, "跳过没有文件路径的 CsfResource 项。");
                continue;
            }

            if (!File.Exists(filePath))
            {
                Log.LogError(string.Empty, "CSF002", string.Empty, string.Empty, 0, 0, 0, 0, "文件不存在：{0}", filePath);
                continue;
            }

            CsfData data;
            try
            {
                data = ReadSource(filePath, item.GetMetadata("Format"));
            }
            catch (Exception ex)
            {
                Log.LogError(string.Empty, "CSF003", string.Empty, string.Empty, 0, 0, 0, 0, "编译失败：{0}：{1}", filePath, ex.Message);
                continue;
            }

            string intermediatePath = GetIntermediatePath(filePath);
            WriteCsf(intermediatePath, data);
            Log.LogMessage(MessageImportance.Normal, "已生成：{0}", intermediatePath);

            compiled.Add(data);
        }

        if (compiled.Count == 0)
        {
            return !Log.HasLoggedErrors;
        }

        CsfData merged = CsfMerger.Merge(compiled);

        WriteCsf(OutputFile, merged);
        Log.LogMessage(MessageImportance.Normal, "已合并生成：{0}", OutputFile);

        return !Log.HasLoggedErrors;
    }

    private static CsfData ReadSource(string filePath, string format)
    {
        return format.ToUpperInvariant() switch
        {
            "CSF" or "" => ReadBinary(filePath),
            "YAMLV1" or "YAML" => ReadYaml(filePath),
            "XMLV1" or "XML" => ReadXml(filePath),
            "JSONV1" => ReadJson(filePath, version: 1),
            "JSONV2" => ReadJson(filePath, version: 2),
            _ => throw new NotSupportedException($"不支持的格式：{format}"),
        };
    }

    private static CsfData ReadBinary(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        return CsfReader.ReadAllData(stream);
    }

    private static CsfData ReadYaml(string filePath)
    {
        using StreamReader reader = new(filePath);
        return CsfYamlV1Reader.Read(reader);
    }

    private static CsfData ReadXml(string filePath)
    {
        using StreamReader reader = new(filePath);
        return CsfXmlV1Reader.Read(reader);
    }

    private static CsfData ReadJson(string filePath, int version)
    {
        using FileStream stream = File.OpenRead(filePath);
        return version switch
        {
            1 => CsfJsonV1.ReadAllDataAsync(stream).GetAwaiter().GetResult(),
            _ => CsfJsonV2.ReadAllDataAsync(stream).GetAwaiter().GetResult(),
        };
    }

    private static void WriteCsf(string path, CsfData data)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = File.Create(path);
        CsfWriter.WriteAllData(stream, data);
    }

    private string GetIntermediatePath(string filePath)
    {
        string relative = MakeRelative(ProjectDirectory, filePath) ?? Path.GetFileName(filePath);

        string intermediate = Path.Combine(IntermediateDirectory, relative);
        if (!intermediate.EndsWith(".csf", StringComparison.OrdinalIgnoreCase))
        {
            intermediate += ".csf";
        }

        return intermediate;
    }

    private static string? MakeRelative(string basePath, string fullPath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return null;
        }

        string root = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string file = Path.GetFullPath(fullPath);

        if (file.Length > root.Length + 1
            && file.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            && (file[root.Length] == Path.DirectorySeparatorChar || file[root.Length] == Path.AltDirectorySeparatorChar))
        {
            return file[(root.Length + 1)..];
        }

        return null;
    }
}
