namespace Shimakaze.Sdk.IO.Serialization;

/// <summary>
/// Serializer.
/// </summary>
/// <typeparam name="TDocument">Document Type.</typeparam>
/// <typeparam name="TOptions">Options Type.</typeparam>
public interface ISerializer<TDocument, TOptions>
{
    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="options">Options.</param>
    /// <returns>Document.</returns>
    static abstract TDocument Deserialize(Stream stream, TOptions? options = default);

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="document">Document.</param>
    /// <param name="options">Options.</param>
    static abstract void Serialize(Stream stream, TDocument document, TOptions? options = default);
}
