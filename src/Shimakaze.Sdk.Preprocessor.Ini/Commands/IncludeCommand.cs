namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

internal sealed class IncludeCommand : IPreprocessorCommand
{
    public string Command { get; } = "include";

    public async Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        if (args.Length != 1)
            throw new Exception("Invalid arguments");

        var workingDirectory =
            preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.WorkingDirectory);
        var currentFile = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile);
        string currentDirectory = workingDirectory.Peek();

        FileInfo file = preprocessor.GetFileFromSourceFileList(args[0].Trim(new[] { '"', '\'' }), currentDirectory);

        currentDirectory = file.Directory!.FullName;

        currentFile.Push(file.FullName);
        Debug.WriteLine($"Push {PreprocessorVariableNames.CurrentFile}: {file.FullName}");

        workingDirectory.Push(currentDirectory);
        Debug.WriteLine($"Push {PreprocessorVariableNames.WorkingDirectory}: {currentDirectory}");

        await preprocessor.ExecuteAsync(file).ConfigureAwait(false);

        string tmp = workingDirectory.Pop();
        Debug.WriteLine($"Pop  {PreprocessorVariableNames.WorkingDirectory}: {tmp}");

        tmp = currentFile.Pop();
        Debug.WriteLine($"Pop  {PreprocessorVariableNames.CurrentFile}: {tmp}");
    }
}