using Shimakaze.Sdk.Preprocessor.Ini;

namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

internal sealed class ElifCommand : IPreprocessorCommand
{
    public string Command { get; } = "elif";

    public Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        switch (args.Length)
        {
            case 1:
                var stack = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack);
                string tmp = stack.Pop();
                Debug.WriteLine($"Pop  DefineStack: {tmp}");
                stack.Push(args[0]);
                Debug.WriteLine($"Push DefineStack: {args[0]}");
                preprocessor.Variables[PreprocessorVariableNames.WriteOutput] =
                    !preprocessor.GetVariable<bool>(PreprocessorVariableNames.WriteOutput)
                    && preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines).Contains(args[0]);
                break;
            default:
                throw new ArgumentException("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}