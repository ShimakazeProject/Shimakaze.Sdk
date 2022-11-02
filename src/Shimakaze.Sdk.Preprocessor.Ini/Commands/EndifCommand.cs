namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

internal sealed class EndifCommand : IPreprocessorCommand
{
    public string Command { get; } = "endif";
    public Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        if (args.Length != 0)
            throw new ArgumentException("Invalid arguments");
        if (preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack).Count == 0)
            throw new Exception("endif without ifdef");

        string tmp = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.DefineStack).Pop();
        Debug.WriteLine($"Pop  DefineStack  {tmp}");

        preprocessor.Variables[PreprocessorVariableNames.WriteOutput] = true;
        return Task.CompletedTask;
    }
}