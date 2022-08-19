
namespace Shimakaze.Sdk.Loader;

public abstract class TextLoader<TDocument, TReadOptions, TWriteOptions> : Loader<TDocument, TReadOptions, TWriteOptions>, ITextLoader<TDocument, TReadOptions, TWriteOptions>
    where TDocument : class
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    public override TDocument Read(Stream stream, TReadOptions? options = default)
        => Read(new StreamReader(stream), options);

    public abstract TDocument Read(TextReader tr, TReadOptions? options = default);

    public override void Write(TDocument document, Stream stream, TWriteOptions? options = default)
        => Write(document, new StreamWriter(stream), options);

    public abstract void Write(TDocument document, TextWriter tw, TWriteOptions? options = default);
}