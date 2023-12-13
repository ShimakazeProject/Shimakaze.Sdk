#if NETSTANDARD2_0
namespace System.IO;

public static class TextWriterExtensions
{
    public static async ValueTask DisposeAsync(this TextWriter stream)
    {
        await Task.Run(stream.Dispose);
    }
}

#endif