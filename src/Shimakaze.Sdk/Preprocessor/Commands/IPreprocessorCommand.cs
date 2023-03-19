namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// PreprocessorCommand Interface
/// </summary>
public interface IPreprocessorCommand
{
    /// <summary>
    /// Execute Command
    /// </summary>
    /// <param name="args">command params</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    Task ExecuteAsync(string[] args, CancellationToken cancellationToken);
}
