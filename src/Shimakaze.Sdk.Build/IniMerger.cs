using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.IO.Ini.Serialization;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Ini 合并器
/// </summary>
public sealed class IniMerger : MSTask
{
    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required ITaskItem[] SourceFiles { get; set; }

    /// <summary>
    /// 生成的文件路径
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
        Log.LogMessage("Merging Ini...");
        if (!DestinationFile.CreateParentDirectory(Log))
            return false;

        IO.Ini.IniMerger merger = new();
        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            using var stream = File.OpenText(file.ItemSpec);
            using IniDeserializer deserializer = new(stream);
            merger.UnionWith(deserializer.Deserialize());
            file.CopyMetadataTo(OutputFile);
        }

        OutputFile.SetMetadata("Pack", "True");
        using Stream output = File.Create(DestinationFile);
        merger.BuildAndWriteTo(output);

        return true;
    }
}
