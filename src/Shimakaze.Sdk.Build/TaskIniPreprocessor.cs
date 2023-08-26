using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Preprocessor;
using Shimakaze.Sdk.Preprocessor.Extensions;

using MSTask = Microsoft.Build.Utilities.Task;
using TaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Ini 预处理器
/// </summary>
public sealed class TaskIniPreprocessor : MSTask
{
    private const string Metadata_Destination = "Destination";

    /// <summary>
    /// 符号
    /// </summary>
    [Required]
    public required string Defines { get; set; }

    /// <summary>
    /// 输出的文件
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// 将要被预处理的文件列表
    /// </summary>
    [Required]
    public required ITaskItem[] SourceFiles { get; set; }

    /// <inheritdoc />
    public override bool Execute()
    {
        var services = new ServiceCollection()
            .AddPreprocessor(i => i.AddDefines(Defines.Split(';').Select(i => i.Trim())))
            .AddRegionCommands()
            .AddConditionCommands()
            .AddDefineCommands()
            .AddSingleton<IList<ITaskItem>>(new List<ITaskItem>(SourceFiles.Length));

        using var provider = services.BuildServiceProvider();

        foreach (var file in SourceFiles)
        {
            var dest = file.GetMetadata(Metadata_Destination);
            if (!dest.CreateParentDirectory(Log))
                return false;

            using var source = File.OpenText(file.ItemSpec);
            using var target = File.CreateText(dest);
            var task = provider.GetRequiredService<IPreprocessor>()
                .ExecuteAsync(source, target, file.ItemSpec);

            TaskItem item = new(dest);
            file.CopyMetadataTo(item);
            item.RemoveMetadata(Metadata_Destination);
            provider.GetRequiredService<IList<ITaskItem>>().Add(item);

            task.Wait();
        }

        OutputFiles = provider.GetRequiredService<IList<ITaskItem>>().ToArray();

        return !Log.HasLoggedErrors;
    }
}