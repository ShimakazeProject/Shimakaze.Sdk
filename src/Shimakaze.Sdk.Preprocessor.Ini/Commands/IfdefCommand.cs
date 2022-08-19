using Shimakaze.Sdk.Preprocessor.Ini;

namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

internal sealed class IfdefCommand : IPreprocessorCommand
{
    public string Command { get; } = "ifdef";

    public Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        switch (args.Length)
        {
            case 1:
                preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack_Stack_String).Push(args[0]);
                Debug.WriteLine($"Push DefineStack: {args[0]}");

                preprocessor.Variables[PreprocessorVariableNames.WriteOutput_Boolean] =
                    preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines_HashSet_String).Contains(args[0]);
                break;
            default:
                throw new Exception("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}