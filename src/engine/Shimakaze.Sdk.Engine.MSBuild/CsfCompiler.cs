using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Engine.Csf;

using Task = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Engine.MSBuild;

/// <summary>
/// CSF 编译器
/// </summary>
public sealed class CsfCompiler : Task
{
    /// <summary>
    /// 待编译的 CSF 源文件列表（带 <c>Format</c> 元数据）。
    /// </summary>
    [Required]
    public ITaskItem[] Sources { get; set; } = [];

    /// <summary>
    /// 最终输出目录（通常位于 bin 下）。
    /// </summary>
    [Required]
    public string OutputPath { get; set; } = Path.Combine("bin");

    /// <summary>
    /// 最终输出的 CSF 文件列表（带 <c>Format</c> 元数据）。
    /// </summary>
    [Output]
    public ITaskItem[]? OutputFiles { get; set; }

    public override bool Execute()
    {
        if (Sources.Length is 0)
        {
            Log.LogMessage(MessageImportance.Low, "没有需要编译的 CSF 文件。");
            return true;
        }
        Directory.CreateDirectory(OutputPath);
        List<ITaskItem> outputs = [];
        foreach (var item in Sources)
        {
            string src = item.GetMetadata("FullPath");
            string fmtSrc = item.GetMetadata("Format");
            string dest = item.GetMetadata("Destination");
            if (!File.Exists(src))
            {
                Log.LogWarning(
                    "CSF",
                    "CSF0001",
                    string.Empty,
                    src,
                    0,
                    0,
                    0,
                    0,
                    "文件不存在：{0}",
                    src);
                continue;
            }

            if (!Enum.TryParse<CsfFormat>(fmtSrc, true, out var fSrc))
            {
                Log.LogError(
                    "CSF",
                    "CSF0002",
                    string.Empty,
                    src,
                    0,
                    0,
                    0,
                    0,
                    "不支持的格式：{0}",
                    fmtSrc);
                continue;
            }

            if (string.IsNullOrWhiteSpace(dest))
            {
                Log.LogError(
                    "CSF",
                    "CSF0003",
                    string.Empty,
                    src,
                    0,
                    0,
                    0,
                    0,
                    "未设置目标文件名：{0}",
                    dest);
                continue;
            }

            try
            {
                using var fs = File.OpenRead(src);
                var data = CsfTool.LoadFrom(fs, fSrc);
                string destFileName = Path.GetFileNameWithoutExtension(src) + ".csf";
                string path = Path.Combine(OutputPath, destFileName);
                using var stream = File.Create(path);
                CsfTool.SaveTo(data, stream, CsfFormat.Csf);
                Log.LogMessage(MessageImportance.Normal, "已编译生成：{0}", path);
                var taskItem = new TaskItem(path);
                taskItem.SetMetadata("Format", CsfFormat.Csf.ToString());
                taskItem.SetMetadata("Destination", dest);
                outputs.Add(taskItem);
            }
            catch (Exception ex)
            {
                Log.LogError(
                    "CSF",
                    "CSF0004",
                    string.Empty,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    "解析失败：{0}：{1}",
                    src,
                    ex.Message);
                Log.LogErrorFromException(
                    ex,
                    true,
                    true,
                    src);
                continue;
            }
        }
        OutputFiles = [.. outputs];
        return !Log.HasLoggedErrors;
    }
}
