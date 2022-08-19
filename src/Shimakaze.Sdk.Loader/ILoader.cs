
namespace Shimakaze.Sdk.Loader;

public interface ILoader<TDocument, TReadOptions, TWriteOptions>
    where TDocument : class
    where TReadOptions : IReadOptions, new()
    where TWriteOptions : IWriteOptions, new()
{
    TDocument Read(Stream stream, TReadOptions? options = default);
    void Write(TDocument document, Stream stream, TWriteOptions? options = default);
}
