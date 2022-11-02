namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

internal sealed class IfndefCommand : IPreprocessorCommand
{
    public string Command { get; } = "ifndef";

    public Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        switch (args.Length)
        {
            case 1:
                preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack).Push(args[0]);
                Debug.WriteLine($"Push DefineStack: {args[0]}");
                preprocessor.Variables[PreprocessorVariableNames.WriteOutput] =
                    !preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines).Contains(args[0]);
                break;
            default:
                throw new ArgumentException("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}