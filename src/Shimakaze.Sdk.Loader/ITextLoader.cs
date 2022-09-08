
namespace Shimakaze.Sdk.Loader;

public interface ITextLoader<TDocument, TReadOptions, TWriteOptions>: ILoader<TDocument, TReadOptions, TWriteOptions>
    where TDocument : class
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    Task<TDocument> ReadAsync(TextReader tr, TReadOptions? options = default);
    Task WriteAsync(TDocument document, TextWriter tw, TWriteOptions? options = default);
}
