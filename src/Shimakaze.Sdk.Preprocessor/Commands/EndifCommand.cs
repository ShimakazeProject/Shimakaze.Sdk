namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class EndifCommand : IPreprocessorCommand
{
    public string Command { get; } = "endif";
    public Task ExecuteAsync(string[] args!!, string currentFilePath!!, Preprocessor preprocessor!!)
    {
        if (args.Length != 0)
            throw new Exception("Invalid arguments");
        if (preprocessor.DefineStack.Count == 0)
            throw new Exception("endif without ifdef");

        string tmp = preprocessor.DefineStack.Pop();
        Debug.WriteLine($"Pop  DefineStack  {tmp}");

        preprocessor.WriteOutput = true;
        return Task.CompletedTask;
    }
}