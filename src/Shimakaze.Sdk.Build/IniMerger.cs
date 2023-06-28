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
    public required ITaskItem[] DestinationFiles { get; set; }

    /// <summary>
    /// 生成的目标文件
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; set; } = Array.Empty<ITaskItem>();
    /// <summary>
    /// Destination
    /// </summary>
    public const string Metadata_Destination = "Destination";
    /// <summary>
    /// Type
    /// </summary>
    public const string Metadata_Type = "Type";
    /// <summary>
    /// Pack
    /// </summary>
    public const string Metadata_Pack = "Pack";
    /// <summary>
    /// Merge
    /// </summary>
    public const string Metadata_Merge = "Merge";

    /// <inheritdoc/>
    public override bool Execute()
    {
        Log.LogMessage("Merging Ini...");
        var outputs = DestinationFiles
            .DistinctBy(i => i.ItemSpec)
            .ToDictionary(i => i.ItemSpec, i => i.GetMetadata(Metadata_Destination))
            .AsReadOnly();

        IList<ITaskItem> list = new List<ITaskItem>();
        foreach (var group in SourceFiles.GroupBy(i => i.GetMetadata(Metadata_Type)))
        {
            if (!outputs[group.Key].CreateParentDirectory(Log))
                continue;

            IO.Ini.IniMerger merger = new();
            TaskItem item = new(outputs[group.Key]);
            foreach (var file in group)
            {
                using var stream = File.OpenText(file.ItemSpec);
                using IniDeserializer deserializer = new(stream);
                merger.UnionWith(deserializer.Deserialize());
                file.CopyMetadataTo(item);
            }

            item.SetMetadata(Metadata_Pack, true.ToString());
            item.RemoveMetadata(Metadata_Merge);
            using Stream output = File.Create(outputs[group.Key]);
            merger.BuildAndWriteTo(output);
            list.Add(item);
        }

        OutputFiles = list.ToArray();

        return !Log.HasLoggedErrors;
    }
}