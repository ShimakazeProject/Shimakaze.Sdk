namespace Shimakaze.Sdk.Preprocessor;

internal static class Program
{
    /// <summary>
    /// Common Preprocessor.
    /// </summary>
    /// <param name="input">Input File</param>
    /// <param name="output">Output File</param>
    /// <param name="defines">Defines</param>
    public static async Task Main(FileInfo input!!, FileInfo output!!, string[]? defines)
    {
        Preprocessor preprocessor = new();

        using var writer = output.CreateText();
        await preprocessor.InitializeAsync(writer, input.DirectoryName!, defines ?? Array.Empty<string>());
        await preprocessor.ExecuteAsync(input.FullName).ConfigureAwait(false);
    }
}