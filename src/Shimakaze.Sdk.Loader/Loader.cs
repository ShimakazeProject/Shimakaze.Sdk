
namespace Shimakaze.Sdk.Loader;

public abstract class Loader<TDocument, TReadOptions, TWriteOptions> : ILoader<TDocument, TReadOptions, TWriteOptions>
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    public abstract Task<TDocument> ReadAsync(Stream stream, TReadOptions? options = default);
    public abstract Task WriteAsync(TDocument document, Stream stream, TWriteOptions? options = default);
}
