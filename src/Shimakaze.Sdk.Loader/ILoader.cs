
namespace Shimakaze.Sdk.Loader;

public interface ILoader<TDocument, TReadOptions, TWriteOptions>
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    Task<TDocument> ReadAsync(Stream stream, TReadOptions? options = default);
    Task WriteAsync(TDocument document, Stream stream, TWriteOptions? options = default);
}
