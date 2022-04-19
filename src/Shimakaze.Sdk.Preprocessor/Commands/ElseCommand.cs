namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class ElseCommand : IPreprocessorCommand
{
    public string Command { get; } = "else";

    public Task ExecuteAsync(string[] args!!, string currentFilePath!!, Preprocessor preprocessor!!)
    {
        string tmp = preprocessor.DefineStack.Pop();
        Debug.WriteLine($"Pop  DefineStack: {tmp}");
        preprocessor.DefineStack.Push("Else");
        Debug.WriteLine("Push DefineStack: Else");
        preprocessor.WriteOutput = !preprocessor.WriteOutput;
        return Task.CompletedTask;
    }
}