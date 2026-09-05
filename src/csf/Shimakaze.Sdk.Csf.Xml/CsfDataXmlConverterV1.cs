using System.Globalization;
using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// Csf文档序列化器
/// </summary>
public sealed class CsfDataXmlConverterV1 : XmlConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData Read(XmlReader reader, XmlSerializerOptions options)
    {
        CsfMetadata metadata = new();

        if (!options.TryGetConverter<CsfLabelXmlConverterV1>(out var converter))
            converter = new();

        while (reader.Read())
        {
            if (reader.NodeType is not XmlNodeType.Element || reader.Name is not "Resources")
                continue;

            if (int.TryParse(reader.GetAttribute("version"), out int version))
                metadata.Version = version;

            if (int.TryParse(reader.GetAttribute("language"), out int language))
                metadata.Language = language;

            break;
        }

        List<CsfLabel> labels = [];

        while (reader.Read())
        {
            if (reader.NodeType is XmlNodeType.Element && reader.Name is "Label")
                labels.Add(converter.Read(reader, options));
            else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Resources")
                break;
        }

        return new()
        {
            Metadata = metadata,
            Labels = labels
        };
    }

    /// <inheritdoc/>
    public override void Write(XmlWriter writer, CsfData value, XmlSerializerOptions options)
    {
        if (!options.TryGetConverter<CsfLabelXmlConverterV1>(out var converter))
            converter = new();

        writer.WriteStartDocument();

        writer.WriteProcessingInstruction(
            "xml-model",
            $"href=\"{XmlConstants.SchemaUrls.V1}\" type=\"application/xml\" schematypens=\"http://www.w3.org/2001/XMLSchema\"");

        writer.WriteStartElement("Resources");
        writer.WriteAttributeString("protocol", "1");
        writer.WriteAttributeString("version", value.Metadata.Version.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("language", value.Metadata.Language.Value.ToString(CultureInfo.InvariantCulture));

        foreach (var item in value.Labels)
            converter.Write(writer, item, options);

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}
