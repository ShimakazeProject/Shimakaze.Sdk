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
public sealed class IniPreprocessor : MSTask
{
    /// <summary>
    /// 将要被预处理的文件列表
    /// </summary>
    [Required]
    public required string Files { get; set; }

    /// <summary>
    /// 符号
    /// </summary>
    [Required]
    public required string Defines { get; set; }

    /// <summary>
    /// 目标目录
    /// </summary>
    [Required]
    public required string TargetDirectory { get; set; }

    /// <summary>
    /// 项目目录
    /// </summary>
    [Required]
    public required string BaseDirectory { get; set; }

    /// <summary>
    /// 输出的文件的地址
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; private set; } = Array.Empty<ITaskItem>();

    /// <inheritdoc/>
    public override bool Execute()
    {
        ServiceCollection services = new();
        services
            .AddPreprocessor(i => i.AddDefines(Defines.Split(';').Select(i => i.Trim())))
            .AddRegionCommands()
            .AddConditionCommands()
            .AddDefineCommands();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IPreprocessor preprocessor = serviceProvider.GetRequiredService<IPreprocessor>();

        List<ITaskItem> outputFiles = new();
        Task.WaitAll(Files.Split(';').Select(i => i.Trim()).Select(Path.GetFullPath).Select(async file =>
        {
            string path = file.Replace(BaseDirectory, TargetDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            path = Path.Combine(
                Path.GetDirectoryName(path)!,
                $"{Path.GetFileNameWithoutExtension(path)}.g.pp{Path.GetExtension(file)}"
            );

            outputFiles.Add(new TaskItem(path));

            using var source = File.OpenText(file);
            await using var target = File.CreateText(path);

            await preprocessor.ExecuteAsync(source, target, file);
        }).ToArray());
        OutputFiles = outputFiles.ToArray();

        return true;
    }
}
