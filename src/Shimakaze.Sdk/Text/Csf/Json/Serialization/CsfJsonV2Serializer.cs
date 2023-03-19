using System.Text.Json;

using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Text.Csf.Json.Converter.V2;
using Shimakaze.Sdk.Text.Serialization;

namespace Shimakaze.Sdk.Text.Csf.Json.Serialization;

/// <summary>
/// CsfJsonV2Serializer.
/// </summary>
public sealed class CsfJsonV2Serializer : ITextSerializer<CsfDocument?, JsonSerializerOptions>
{
    private static JsonSerializerOptions Init(ref JsonSerializerOptions? options)
    {
        options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            options.Converters.Add(item);
        return options;
    }

    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="reader">reader.</param>
    /// <param name="options">options.</param>
    /// <returns><see cref="CsfDocument"/>.</returns>
    public static CsfDocument? Deserialize(TextReader reader, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<CsfDocument>(reader.ReadToEnd(), Init(ref options));
    }

    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="options">options.</param>
    /// <returns><see cref="CsfDocument"/>.</returns>
    public static CsfDocument? Deserialize(Stream stream, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<CsfDocument>(stream, Init(ref options));
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="writer">writer.</param>
    /// <param name="document">document.</param>
    /// <param name="options">options.</param>
    public static void Serialize(TextWriter writer, CsfDocument? document, JsonSerializerOptions? options = null)
    {
        writer.Write(JsonSerializer.Serialize(document, Init(ref options)));
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="document">document.</param>
    /// <param name="options">options.</param>
    public static void Serialize(Stream stream, CsfDocument? document, JsonSerializerOptions? options = null)
    {
        JsonSerializer.Serialize(stream, document, Init(ref options));
    }
}
