namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// PreprocessorHost
/// </summary>
public interface IPreprocessor
{
    /// <summary>
    /// ExecuteAsync
    /// </summary>
    /// <param name="input"> 输入文件 </param>
    /// <param name="output"> 输出文件 </param>
    /// <param name="filePath"> 输入文件的地址 </param>
    /// <param name="cancellationToken"> </param>
    /// <returns> </returns>
    Task ExecuteAsync(
        TextReader input,
        TextWriter output,
        string filePath,
        CancellationToken cancellationToken = default
    );
}