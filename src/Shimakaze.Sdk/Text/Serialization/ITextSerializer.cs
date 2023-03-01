using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.Text.Serialization;

/// <summary>
/// Text serializer.
/// </summary>
/// <typeparam name="TDocument">Document Type.</typeparam>
/// <typeparam name="TOptions">Options Type.</typeparam>
public interface ITextSerializer<TDocument, TOptions> : ISerializer<TDocument, TOptions>
{
    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="reader">Text Reader.</param>
    /// <param name="options">Options.</param>
    /// <returns>Document.</returns>
    static abstract TDocument Deserialize(TextReader reader, TOptions? options = default);

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="writer">Text Writer.</param>
    /// <param name="document">Document.</param>
    /// <param name="options">Options.</param>
    static abstract void Serialize(TextWriter writer, TDocument document, TOptions? options = default);
}
