using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// Csf 值 Xml 转换器
/// </summary>
public sealed class CsfValueXmlConverterV1 : XmlConverter<CsfValue>
{
    /// <inheritdoc/>
    public override CsfValue Read(XmlReader reader, XmlSerializerOptions options)
    {
        if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA)
            return new(reader.Value, null);

        string? extra = reader.GetAttribute("extra");
        string value = reader.ReadElementContentAsString();

        return new(value, extra);
    }

    /// <inheritdoc/>
    public override void Write(XmlWriter writer, CsfValue value, XmlSerializerOptions options)
    {
        writer.WriteStartElement("Value");

        if (value.Extra is not null)
            writer.WriteAttributeString("extra", value.Extra);

        writer.WriteString(value.Value);
        writer.WriteEndElement();
    }
}
