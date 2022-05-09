namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class ElifCommand : IPreprocessorCommand
{
    public string Command { get; } = "elif";

    public Task ExecuteAsync(string[] args!!, Preprocessor preprocessor!!)
    {
        switch (args.Length)
        {
            case 1:
                var stack = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack_Stack_String);
                string tmp = stack.Pop();
                Debug.WriteLine($"Pop  DefineStack: {tmp}");
                stack.Push(args[0]);
                Debug.WriteLine($"Push DefineStack: {args[0]}");
                preprocessor.Variables[PreprocessorVariableNames.WriteOutput_Boolean] =
                    !preprocessor.GetVariable<bool>(PreprocessorVariableNames.WriteOutput_Boolean)
                    && preprocessor.GetVariable<HashSet<string>>(PreprocessorVariableNames.Defines_HashSet_String).Contains(args[0]);
                break;
            default:
                throw new ArgumentException("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}