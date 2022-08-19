using Shimakaze.Sdk.Preprocessor.Ini;
using Shimakaze.Sdk.Utils;

namespace Shimakaze.Sdk.Preprocessor.Ini.Commands;

internal sealed class IncludeCommand : IPreprocessorCommand
{
    public string Command { get; } = "include";

    public async Task ExecuteAsync(string[] args, IniPreprocessor preprocessor)
    {
        if (args.Length != 1)
            throw new Exception("Invalid arguments");

        var workingDirectory = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.WorkingDirectory_Stack_String);
        var currentFile = preprocessor.GetVariable<Stack<string>>(PreprocessorVariableNames.CurrentFile_Stack_String);
        string currentDirectory = workingDirectory.Peek();

        string filePath = args[0].Trim(new[] { '"', '\'' });
        if (!Path.IsPathRooted(filePath))
            filePath = Path.Combine(currentDirectory, filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        currentDirectory = Path.GetDirectoryName(filePath) ?? throw new Exception("Invalid file path");


        currentFile.Push(filePath);
        Debug.WriteLine($"Push {PreprocessorVariableNames.CurrentFile_Stack_String}: {filePath}");

        workingDirectory.Push(currentDirectory);
        Debug.WriteLine($"Push {PreprocessorVariableNames.WorkingDirectory_Stack_String}: {currentDirectory}");

        await preprocessor.ExecuteAsync(filePath).ConfigureAwait(false);

        string tmp = workingDirectory.Pop();
        Debug.WriteLine($"Pop  {PreprocessorVariableNames.WorkingDirectory_Stack_String}: {tmp}");

        tmp = currentFile.Pop();
        Debug.WriteLine($"Pop  {PreprocessorVariableNames.CurrentFile_Stack_String}: {tmp}");
    }
}
