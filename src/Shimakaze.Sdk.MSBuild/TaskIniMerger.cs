using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Ini;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Ini 合并器
/// </summary>
public sealed class TaskIniMerger : MSTask
{
    /// <summary>
    /// Pack
    /// </summary>
    public const string MetadataPack = "Pack";

    // TODO: Support Ares INI
    ///// <summary>
    ///// SpecialType
    ///// </summary>
    //public const string MetadataSpecialType = "SpecialType";

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
        Log.LogMessage("Merging Ini...");
        if (!DestinationFile.CreateParentDirectory(Log))
            return false;

        IniDocument ini = [];
        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            using var sr = File.OpenText(file.ItemSpec);
            using IniTokenReader reader = new(sr);
            using IniDocumentBinder binder = new(reader);
            ini = binder.Bind(ini);
            file.CopyMetadataTo(OutputFile);
        }

        OutputFile.SetMetadata(MetadataPack, true.ToString());
        using var output = File.CreateText(DestinationFile);
        using IniTokenWriter writer = new(output);
        writer.Write(ini);
        output.Flush();

        return !Log.HasLoggedErrors;
    }
}