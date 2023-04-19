using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf文档序列化器
/// </summary>
public class CsfDocumentXmlSerializer : IXmlSerializer<CsfDocument>
{
    private readonly CsfDataListXmlSerializer _csfDataListXmlSerializer = new();

    /// <inheritdoc/>
    public CsfDocument Deserialize(XmlReader reader)
    {
        CsfMetadata head = new(0, 0);
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.Name is "Resources":
                    if (int.TryParse(reader.GetAttribute("version"), out int v))
                        head.Version = v;

                    if (int.TryParse(reader.GetAttribute("language"), out int l))
                        head.Language = l;
                    goto outer;
            }
        }

    outer:
        return new()
        {
            Metadata = head,
            Data = _csfDataListXmlSerializer.Deserialize(reader)
        };
    }

    /// <inheritdoc/>
    public void Serialize(XmlWriter writer, CsfDocument value)
    {
        writer.WriteStartDocument();
        writer.WriteProcessingInstruction("xml-model", $"href=\"{XmlConstants.SchemaUrls.V1}\" type=\"application/xml\" schematypens=\"http://www.w3.org/2001/XMLSchema\"");

        // <Resources protocol="1" version="3" language="0">
        writer.WriteStartElement("Resources");
        writer.WriteAttributeString("protocol", "1");
        writer.WriteAttributeString("version", value.Metadata.Version.ToString());
        writer.WriteAttributeString("language", value.Metadata.Language.ToString());

        _csfDataListXmlSerializer.Serialize(writer, value.Data);

        // </Resources>
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
}