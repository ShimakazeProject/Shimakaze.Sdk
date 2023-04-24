using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.IO.Csf.Serialization;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 合并器
/// </summary>
public sealed class CsfMerger : MSTask
{
    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required ITaskItem[] SourceFiles { get; set; }

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

    /// <inheritdoc/>
    public override bool Execute()
    {
        Log.LogMessage("Merging Csf...");
        if (!DestinationFile.CreateParentDirectory(Log))
            return false;

        IO.Csf.CsfMerger merger = new();
        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            try
            {
                using Stream stream = File.OpenRead(file.ItemSpec);
                using CsfDeserializer deserializer = new(stream);
                merger.UnionWith(deserializer.Deserialize());
                file.CopyMetadataTo(OutputFile);
            }
            catch (Exception ex)
            {
                Log.LogError(
                    "Shimakaze.Sdk.Csf",
                    "CSF0004",
                    "Merge Failed",
                    file.ItemSpec,
                    0,
                    0,
                    0,
                    0,
                    "Cannot Merge file into CSF.");
                Log.LogErrorFromException(ex);
            }
        }

        OutputFile.SetMetadata("Pack", "True");
        using Stream output = File.Create(DestinationFile);
        merger.BuildAndWriteTo(output);

        return true;
    }
}
