namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class IfndefCommand : IPreprocessorCommand
{
    public string Command { get; } = "ifndef";

    public Task ExecuteAsync(string[] args!!, string currentFilePath!!, Preprocessor preprocessor!!)
    {
        switch (args.Length)
        {
            case 1:
                preprocessor.DefineStack.Push(args[0]);
                Debug.WriteLine($"Push DefineStack: {args[0]}");
                preprocessor.WriteOutput = !preprocessor.Defines.Contains(args[0]);
                break;
            default:
                throw new Exception("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}