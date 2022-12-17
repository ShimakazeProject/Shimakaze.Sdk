using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Serialization;
using Shimakaze.Sdk.Text.Json.Csf.Converter.V1;

namespace Shimakaze.Sdk.Text.Json.Csf.Serialization;

/// <summary>
/// CsfJsonV1Serializer.
/// </summary>
public sealed class CsfJsonV1Serializer : ITextSerializer<CsfDocument?, CsfJsonSerializerOptions>
{
    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="reader">reader.</param>
    /// <param name="options">options.</param>
    /// <returns><see cref="CsfDocument"/>.</returns>
    public static CsfDocument? Deserialize(TextReader reader, CsfJsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<CsfDocument>(reader.ReadToEnd(), CsfJsonConverterUtils.CsfJsonSerializerOptions);
    }

    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="options">options.</param>
    /// <returns><see cref="CsfDocument"/>.</returns>
    public static CsfDocument? Deserialize(Stream stream, CsfJsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<CsfDocument>(stream, CsfJsonConverterUtils.CsfJsonSerializerOptions);
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="writer">writer.</param>
    /// <param name="document">document.</param>
    /// <param name="options">options.</param>
    public static void Serialize(TextWriter writer, CsfDocument? document, CsfJsonSerializerOptions? options = null)
    {
        options ??= CsfJsonSerializerOptions.Default;
        JsonSerializerOptions opt = CsfJsonConverterUtils.CsfJsonSerializerOptions;
        opt.WriteIndented = options.WriteIndented;
        writer.Write(JsonSerializer.Serialize(document, opt));
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="document">document.</param>
    /// <param name="options">options.</param>
    public static void Serialize(Stream stream, CsfDocument? document, CsfJsonSerializerOptions? options = null)
    {
        options ??= CsfJsonSerializerOptions.Default;
        JsonSerializerOptions opt = CsfJsonConverterUtils.CsfJsonSerializerOptions;
        opt.WriteIndented = options.WriteIndented;
        JsonSerializer.Serialize(stream, document, opt);
    }
}
