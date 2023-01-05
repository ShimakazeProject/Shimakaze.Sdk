namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Kernel;

/// <summary>
/// PreprocessorHost
/// </summary>
public interface IPreprocessor
{
    /// <summary>
    /// ExecuteAsync
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="filePath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(
        TextReader input,
        TextWriter output,
        string filePath,
        CancellationToken cancellationToken
    );
}
