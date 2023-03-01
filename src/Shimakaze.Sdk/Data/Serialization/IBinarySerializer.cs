using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.Data.Serialization;

/// <summary>
/// Binary serializer.
/// </summary>
/// <typeparam name="TDocument">Document Type.</typeparam>
/// <typeparam name="TOptions">Options Type.</typeparam>
public interface IBinarySerializer<TDocument, TOptions> : ISerializer<TDocument, TOptions>
{
    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="reader">Binary Reader.</param>
    /// <param name="options">Options.</param>
    /// <returns>Document.</returns>
    static abstract TDocument Deserialize(BinaryReader reader, TOptions? options = default);

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="writer">Binary Writer.</param>
    /// <param name="document">Document.</param>
    /// <param name="options">Options.</param>
    static abstract void Serialize(BinaryWriter writer, TDocument document, TOptions? options = default);
}
