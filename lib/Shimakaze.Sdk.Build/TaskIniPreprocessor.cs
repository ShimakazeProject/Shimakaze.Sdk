using System.Collections.Immutable;

using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Commands;
using Shimakaze.Sdk.Preprocessor.Kernel;

using MSTask = Microsoft.Build.Utilities.Task;
using TaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Ini 预处理器
/// </summary>
public sealed class TaskIniPreprocessor : MSTask
{
    /// <summary>
    /// 生成的中间文件的路径
    /// </summary>
    public const string Metadata_Intermediate = "Intermediate";

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
            .AddLogging(builder => builder.AddSimpleConsole())
            .AddEngine((options, services) =>
            {
                options.Defines = new(Defines.Split(';').Select(i => i.Trim()));
                options.Commands = new[]{
                    services.AddCommands<ConditionalCommand>(),
                    services.AddCommands<DefineCommand>(),
                    services.AddCommands<RegionCommand>(),
                    services.AddCommands<TypeCommand>(),
                }.SelectMany(i => i).ToImmutableArray();
            });

        using var provider = services.BuildServiceProvider();

        List<ITaskItem> outputs = new(SourceFiles.Length);

        foreach (var file in SourceFiles)
        {
            try
            {
                var dest = file.GetMetadata(Metadata_Intermediate);
                if (!dest.CreateParentDirectory(Log))
                    return false;

                using var source = File.OpenText(file.ItemSpec);
                using var target = File.CreateText(dest);
                provider.GetRequiredService<Engine>()
                    .Execute(source, target, file.ItemSpec);

                TaskItem item = new(dest);
                file.CopyMetadataTo(item);
                item.RemoveMetadata(Metadata_Intermediate);
                outputs.Add(item);
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }
        }

        OutputFiles = outputs.ToArray();

        return !Log.HasLoggedErrors;
    }
}