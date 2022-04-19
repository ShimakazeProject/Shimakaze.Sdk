namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class DefineCommand : IPreprocessorCommand
{
    public string Command { get; } = "define";

    public Task ExecuteAsync(string[] args!!, string currentFilePath!!, Preprocessor preprocessor!!)
    {
        switch (args.Length)
        {
            case 1:
                preprocessor.Defines.Add(args[0]);
                break;
            default:
                throw new Exception("Invalid arguments");
        }
        return Task.CompletedTask;
    }
}
