
namespace Shimakaze.Sdk.Loader;

public abstract class Loader<TDocument, TReadOptions, TWriteOptions> : ILoader<TDocument, TReadOptions, TWriteOptions>
    where TDocument : class
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    public abstract TDocument Read(Stream stream, TReadOptions? options = default);
    public abstract void Write(TDocument document, Stream stream, TWriteOptions? options = default);
}
