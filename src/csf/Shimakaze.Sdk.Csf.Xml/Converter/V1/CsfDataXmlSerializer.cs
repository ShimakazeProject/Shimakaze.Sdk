using System.Globalization;
using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf文档序列化器
/// </summary>
public class CsfDataXmlSerializer : IXmlSerializer<CsfData>
{
    private readonly CsfLabelListXmlSerializer _csfLabelListXmlSerializer = new();

    /// <inheritdoc />
    public CsfData Deserialize(XmlReader reader)
    {
        CsfMetadata head = new();
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.Name is "Resources":
                    if (int.TryParse(reader.GetAttribute("version"), out int v))
                    {
                        head.Version = v;
                    }

                    if (int.TryParse(reader.GetAttribute("language"), out int l))
                    {
                        head.Language = l;
                    }

                    goto outer;
            }
        }

    outer:
        return new()
        {
            Metadata = head,
            Labels = [.. _csfLabelListXmlSerializer.Deserialize(reader)]
        };
    }

    /// <inheritdoc />
    public void Serialize(XmlWriter writer, CsfData value)
    {
        writer.WriteStartDocument();
        writer.WriteProcessingInstruction("xml-model", $"href=\"{XmlConstants.SchemaUrls.V1}\" type=\"application/xml\" schematypens=\"http://www.w3.org/2001/XMLSchema\"");

        // <Resources protocol="1" version="3" language="0">
        writer.WriteStartElement("Resources");
        writer.WriteAttributeString("protocol", "1");
        writer.WriteAttributeString("version", value.Metadata.Version.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("language", value.Metadata.Language.Value.ToString(CultureInfo.InvariantCulture));

        _csfLabelListXmlSerializer.Serialize(writer, value.Labels);

        // </Resources>
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}
