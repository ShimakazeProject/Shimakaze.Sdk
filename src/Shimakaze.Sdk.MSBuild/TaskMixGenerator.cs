using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Mix;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Mix Packer Task
/// </summary>
public sealed class TaskMixGenerator : MSTask
{
    /// <summary>
    /// 生成的文件
    /// </summary>
    [Required]
    public required string DestinationFile { get; set; }

    /// <summary>
    /// 是否使用旧的ID计算器
    /// </summary>
    [Obsolete("Use IsTD")]
    public bool UsingOldIdCalculater
    {
        get => IsTD;
        set => IsTD = value;
    }

    /// <summary>
    /// 使用适用于 Red Alert 或 Tiberian Down 的游戏引擎的ID计算器
    /// </summary>
    public bool IsTD { get; set; }

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
        Log.LogMessage("Generating Mix...");
        if (!DestinationFile.CreateParentDirectory(Log))
            return false;

        MixBuilder builder = new(IsTD
            ? IdCalculaters.TDIdCalculater
            : IdCalculaters.TSIdCalculater);

        OutputFile = new TaskItem(DestinationFile);
        foreach (var file in SourceFiles)
        {
            Log.LogMessage(MessageImportance.Low, $"Add \"{file.ItemSpec}\" into mix.");
            builder.Files.Add(new(file.ItemSpec));
        }

        using var output = File.Create(DestinationFile);
        builder.BuildAsync(output).Wait();
        output.Flush();

        return !Log.HasLoggedErrors;
    }
}