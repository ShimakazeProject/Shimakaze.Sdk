using Shimakaze.Sdk.Preprocessor.Ini;

namespace Shimakaze.Sdk.Preprocessor;

internal static class Program
{
    /// <summary>
    /// Common Preprocessor.
    /// </summary>
    /// <param name="inputs">Input File</param>
    /// <param name="entry">Entry File</param>
    /// <param name="output">Output File</param>
    /// <param name="defines">Defines</param>
    /// <param name="extensions">Extensions</param>
    public static async Task Main(FileInfo[] inputs, FileInfo entry, FileInfo output, string[]? defines,
        string[] extensions)
    {
        IniPreprocessor preprocessor = new();

        await using var writer = output.CreateText();
        await preprocessor.InitializeAsync(writer, entry.DirectoryName!, inputs, defines ?? Array.Empty<string>(),
            extensions);
        await preprocessor.ExecuteAsync(entry).ConfigureAwait(false);
    }
}