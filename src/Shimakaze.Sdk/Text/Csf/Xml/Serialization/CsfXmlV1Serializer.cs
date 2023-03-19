using System.Xml;

using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Text.Csf.Xml.Converter.V1;
using Shimakaze.Sdk.Text.Serialization;

namespace Shimakaze.Sdk.Text.Csf.Xml.Serialization;

/// <summary>
/// CSF Xml Serializer.
/// </summary>
public class CsfXmlV1Serializer : ITextSerializer<CsfDocument, object>
{
    /// <inheritdoc/>
    public static CsfDocument Deserialize(TextReader reader, object? options = null)
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(reader);
        return serializer.Deserialize(xmlReader);
    }

    /// <inheritdoc/>
    public static CsfDocument Deserialize(Stream stream, object? options = null)
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(stream);
        return serializer.Deserialize(xmlReader);
    }

    /// <inheritdoc/>
    public static void Serialize(TextWriter writer, CsfDocument document, object? options = null)
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(writer);
        serializer.Serialize(xmlWriter, document);
    }

    /// <inheritdoc/>
    public static void Serialize(Stream stream, CsfDocument document, object? options = null)
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(stream);
        serializer.Serialize(xmlWriter, document);
    }
}
