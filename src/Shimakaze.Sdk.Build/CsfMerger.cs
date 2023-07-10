using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.IO.Csf;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 合并器
/// </summary>
public sealed class CsfMerger : MSTask
{
    private const string Metadata_Pack = "Pack";

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

        IO.Csf.CsfMerger merger = new();
        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            using Stream stream = File.OpenRead(file.ItemSpec);
            using CsfReader reader = new(stream);
            merger.UnionWith(reader.ReadAsync().Result.Data);
            file.CopyMetadataTo(OutputFile);
        }

        OutputFile.SetMetadata(Metadata_Pack, true.ToString());
        using Stream output = File.Create(DestinationFile);
        merger.BuildAndWriteToAsync(output).Wait();

        return true;
    }
}