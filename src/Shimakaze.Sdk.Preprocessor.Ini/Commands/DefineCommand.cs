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
    /// <param name="args">�������</param>
    /// <param name="preprocessor">Ԥ������ʵ��</param>
    /// <exception cref="ArgumentException">��������ȷʱ�׳�</exception>
    public Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        switch (args.Length)
        {
            case 1:
                preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines).Add(args[0]);
                break;
            default:
                throw new ArgumentException("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}
