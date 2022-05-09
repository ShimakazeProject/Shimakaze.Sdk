namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class ElseCommand : IPreprocessorCommand
{
    public string Command { get; } = "else";

    public Task ExecuteAsync(string[] args!!, Preprocessor preprocessor!!)
    {
        var stack = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack_Stack_String);
        string tmp = stack.Pop();
        Debug.WriteLine($"Pop  DefineStack: {tmp}");
        stack.Push("Else");
        Debug.WriteLine("Push DefineStack: Else");
        preprocessor.Variables[PreprocessorVariableNames.WriteOutput_Boolean] =
            !preprocessor.GetVariable<bool>(PreprocessorVariableNames.WriteOutput_Boolean);
        return Task.CompletedTask;
    }
}