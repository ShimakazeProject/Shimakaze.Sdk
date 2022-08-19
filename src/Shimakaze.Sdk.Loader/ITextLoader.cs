
namespace Shimakaze.Sdk.Loader;

public interface ITextLoader<TDocument, TReadOptions, TWriteOptions>: ILoader<TDocument, TReadOptions, TWriteOptions>
    where TDocument : class
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    TDocument Read(TextReader tr, TReadOptions? options = default);
    void Write(TDocument document, TextWriter tw, TWriteOptions? options = default);
}
