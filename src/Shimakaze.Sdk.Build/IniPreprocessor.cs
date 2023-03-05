using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Compiler.Preprocessor;
using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;
using Shimakaze.Sdk.IO.Mix;

using MSTask = Microsoft.Build.Utilities.Task;

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

        Task.WaitAll(Files.Split(';').Select(i => i.Trim()).Select(Path.GetFullPath).Select(async file =>
        {
            using var source = File.OpenText(file);
            await using var target = File.CreateText(
                Path.GetFileNameWithoutExtension(file.Replace(BaseDirectory, TargetDirectory))
                + ".g.pp"
                + Path.GetExtension(file)
            );

            await preprocessor.ExecuteAsync(source, target, file);
        }).ToArray());

        return true;
    }
}
