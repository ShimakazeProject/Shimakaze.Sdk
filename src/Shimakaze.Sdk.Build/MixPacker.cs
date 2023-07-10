﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.IO.Mix;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Mix Packer Task
/// </summary>
public sealed class MixPacker : MSTask
{
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
        Log.LogMessage("Packing Mix...");
        if (!DestinationFile.CreateParentDirectory(Log))
            return false;

        var builder = new MixBuilder() { IdCalculater = IdCalculaters.TSIdCalculater };
        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            Log.LogMessage(MessageImportance.Low, $"Add \"{file.ItemSpec}\" into mix.");
            builder.AddFile(new(file.ItemSpec));
        }

        using var output = File.Create(DestinationFile);
        builder.BuildAsync(output).Wait();

        return true;
    }
}