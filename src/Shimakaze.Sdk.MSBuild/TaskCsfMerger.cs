using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Csf;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

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
            return false;

        CsfSet merger = [];
        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            using Stream stream = File.OpenRead(file.ItemSpec);
            merger.UnionWith(CsfReader.Read(stream).Data);
            file.CopyMetadataTo(OutputFile);
        }

        OutputFile.SetMetadata(MetadataPack, true.ToString());
        using Stream output = File.Create(DestinationFile);
        merger.BuildAndWriteTo(output);
        output.Flush();

        return !Log.HasLoggedErrors;
    }
}