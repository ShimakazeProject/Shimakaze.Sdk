
namespace Shimakaze.Sdk.Loader;

public abstract class TextLoader<TDocument, TReadOptions, TWriteOptions> :
    Loader<TDocument, TReadOptions, TWriteOptions>,
    ITextLoader<TDocument, TReadOptions, TWriteOptions>
    where TDocument : class
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    public override Task<TDocument> ReadAsync(Stream stream, TReadOptions? options = default)
        => ReadAsync(new StreamReader(stream), options);

    public abstract Task<TDocument> ReadAsync(TextReader tr, TReadOptions? options = default);

    public override Task WriteAsync(TDocument document, Stream stream, TWriteOptions? options = default)
        => WriteAsync(document, new StreamWriter(stream), options);

    public abstract Task WriteAsync(TDocument document, TextWriter tw, TWriteOptions? options = default);
}