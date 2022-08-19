using Shimakaze.Sdk.Preprocessor.Ini;

namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

/// <summary>
/// # Define
/// </summary>
public sealed class DefineCommand : IPreprocessorCommand
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "define";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <param name="preprocessor">预处理器实例</param>
    /// <exception cref="ArgumentException">参数不正确时抛出</exception>
    public Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        switch (args.Length)
        {
            case 1:
                preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines_HashSet_String).Add(args[0]);
                break;
            default:
                throw new ArgumentException("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}
