namespace Shimakaze.Sdk.Preprocessor;

internal interface IPreprocessorCommand
{
    string Command { get; }
    Task InitializeAsync(Preprocessor preprocessor!!) => Task.CompletedTask;
    Task ExecuteAsync(string[] args, string currentFilePath, Preprocessor preprocessor);
}

