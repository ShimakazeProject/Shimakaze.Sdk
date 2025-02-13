using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.IO;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.MSBuild;

/// <summary>
/// Csf 合并器
/// </summary>
public sealed class TaskCsfMerger : MSTask
{
    /// <summary>
    /// Pack
    /// </summary>
    public const string MetadataPack = "Pack";

    /// <summary>
    /// 生成的文件
    /// </summary>
    [Required]
    public required string DestinationFile { get; set; }

    /// <summary>
    /// 生成的目标文件
    /// </summary>
    [Output]
    public ITaskItem? OutputFile { get; set; }

    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required ITaskItem[] SourceFiles { get; set; }

    /// <inheritdoc />
    public override bool Execute()
    {
        Log.LogMessage("Merging Csf...");
        if (!DestinationFile.CreateParentDirectory(Log))
        {
            return false;
        }

        Dictionary<string, CsfLabel> map = [];
        OutputFile = new TaskItem(DestinationFile);
        int? version = null;
        CsfLanguage? language = null;
        foreach (ITaskItem file in SourceFiles)
        {
            using Stream stream = File.OpenRead(file.ItemSpec);
            var csf = CsfReader.ReadAllData(stream);
            version ??= csf.Metadata.Version;
            language ??= csf.Metadata.Language;
            if (language != csf.Metadata.Language)
            {
                Log.LogError(
                    "Shimakaze.Sdk.Csf",
                    "CSF0004",
                    "Inconsistent language",
                    file.ItemSpec,
                    0,
                    0,
                    0,
                    0,
                    "Only allow merging CSFs of the same language");
                continue;
            }
            if (version != csf.Metadata.Version)
            {
                Log.LogError(
                    "Shimakaze.Sdk.Csf",
                    "CSF0004",
                    "Inconsistent version",
                    file.ItemSpec,
                    0,
                    0,
                    0,
                    0,
                    "Only allow merging CSFs of the same version");
                continue;
            }

            csf.Labels
                .ForEach(label => map.TryAdd(label.Name, label));
            file.CopyMetadataTo(OutputFile);
        }
        OutputFile.SetMetadata(MetadataPack, true.ToString());
        using Stream output = File.Create(DestinationFile);
        CsfWriter.WriteAllData(
            output,
            new(
                new()
                {
                    Version = version ?? 3,
                    Language = language ?? 0,
                },
                [.. map.Values]));

        output.Flush();

        return !Log.HasLoggedErrors;
    }
}
