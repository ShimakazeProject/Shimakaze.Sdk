namespace Shimakaze.Sdk.Preprocessor.Commands;

internal sealed class IncludeCommand : IPreprocessorCommand
{
    public string Command { get; } = "include";

    public async Task ExecuteAsync(string[] args!!, string currentFilePath!!, Preprocessor preprocessor!!)
    {
        if (args.Length != 1)
            throw new Exception("Invalid arguments");

        string currentDirectory = preprocessor.WorkingDirectory.Peek();

        string filePath = args[0].Trim(new[] { '"', '\'' });
        if (!Path.IsPathRooted(filePath))
            filePath = Path.Combine(currentDirectory, filePath);

        if (!File.Exists(filePath))
            throw new Exception($"File not found: {filePath}");

        currentDirectory = Path.GetDirectoryName(filePath) ?? throw new Exception("Invalid file path");

        preprocessor.WorkingDirectory.Push(currentDirectory);
        Debug.WriteLine($"Push WorkingDirectory: {currentDirectory}");
        await preprocessor.ExecuteAsync(filePath).ConfigureAwait(false);
        string tmp = preprocessor.WorkingDirectory.Pop();
        Debug.WriteLine($"Pop  WorkingDirectory: {tmp}");
    }
}
