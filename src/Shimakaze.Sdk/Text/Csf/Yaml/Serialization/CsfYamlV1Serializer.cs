using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Text.Csf.Yaml.Converter.V1;
using Shimakaze.Sdk.Text.Serialization;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Text.Csf.Yaml.Serialization;

/// <summary>
/// CSF YAML Serializer.
/// </summary>
public sealed class CsfYamlV1Serializer : ITextSerializer<CsfDocument, object>
{
    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="reader">reader.</param>
    /// <param name="options">options.</param>
    /// <returns><see cref="CsfDocument"/>.</returns>
    public static CsfDocument Deserialize(TextReader reader, object? options = null)
    {
        return new DeserializerBuilder()
          .WithTypeConverter(CsfValueConverter.Instance)
          .WithTypeConverter(CsfDataConverter.Instance)
          .WithTypeConverter(CsfDocumentConverter.Instance)
          .Build()
          .Deserialize<CsfDocument>(reader);
    }

    /// <summary>
    /// Deserialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="options">options.</param>
    /// <returns><see cref="CsfDocument"/>.</returns>
    public static CsfDocument Deserialize(Stream stream, object? options = null)
    {
        using StreamReader sr = new(stream, leaveOpen: true);
        return Deserialize(sr, options);
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="writer">writer.</param>
    /// <param name="document"><see cref="CsfDocument"/>.</param>
    /// <param name="options">options.</param>
    public static void Serialize(TextWriter writer, CsfDocument document, object? options = null)
    {
        new SerializerBuilder()
          .WithTypeConverter(CsfValueConverter.Instance)
          .WithTypeConverter(CsfDataConverter.Instance)
          .WithTypeConverter(CsfDocumentConverter.Instance)
          .Build()
          .Serialize(writer, document);
    }

    /// <summary>
    /// Serialize.
    /// </summary>
    /// <param name="stream">stream.</param>
    /// <param name="document"><see cref="CsfDocument"/>.</param>
    /// <param name="options">options.</param>
    public static void Serialize(Stream stream, CsfDocument document, object? options = null)
    {
        using StreamWriter sw = new(stream, leaveOpen: true);
        Serialize(sw, document, options);
    }
}
