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
    /// 目标文件
    /// </summary>
    [Required]
    public required string TargetFiles { get; set; }

    /// <inheritdoc/>
    public override bool Execute()
    {
        var inputs = Files.Split(';');
        var outputs = TargetFiles.Split(';');
        if (inputs.Length != outputs.Length)
        {
            Log.LogError("InputPaths.Length are not equal that OutputPaths.Length");
            return false;
        }

        ServiceCollection services = new();
        services
            .AddPreprocessor(i => i.AddDefines(Defines.Split(';').Select(i => i.Trim())))
            .AddRegionCommands()
            .AddConditionCommands()
            .AddDefineCommands();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IPreprocessor preprocessor = serviceProvider.GetRequiredService<IPreprocessor>();

        for (int i = 0; i < inputs.Length; i++)
        {
            var outdir = Path.GetDirectoryName(outputs[i]);
            if (string.IsNullOrEmpty(outdir))
            {
                Log.LogError("Unknown Error");
                break;
            }
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);

            using var source = File.OpenText(inputs[i]);
            using var target = File.CreateText(outputs[i]);

            preprocessor.ExecuteAsync(source, target, inputs[i]).Wait();
        }

        return !Log.HasLoggedErrors;
    }
}
