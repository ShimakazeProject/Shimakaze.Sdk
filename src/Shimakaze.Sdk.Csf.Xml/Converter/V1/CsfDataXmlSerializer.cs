using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf数据序列化器
/// </summary>
public class CsfDataXmlSerializer : IXmlSerializer<CsfData>
{
    private readonly CsfValueListXmlSerializer _csfValueListXmlSerializer = new();
    private readonly CsfValueXmlSerializer _csfValueXmlSerializer = new();

    /// <inheritdoc />
    public CsfData Deserialize(XmlReader reader)
    {
        CsfData label = new();
        if (reader.NodeType is XmlNodeType.Element && reader.Name is "Label")
        {
            string? lbl = reader.GetAttribute("name");
            if (!string.IsNullOrWhiteSpace(lbl))
                label.LabelName = lbl;

            label.Values = reader.GetAttribute("extra") switch
            {
                not null => new[] { _csfValueXmlSerializer.Deserialize(reader) },
                _ => _csfValueListXmlSerializer.Deserialize(reader).ToArray(),
            };
        }
        return label;
    }

    /// <inheritdoc />
    public void Serialize(XmlWriter writer, CsfData value)
    {
        // <Label name="label_name">
        writer.WriteStartElement("Label");
        writer.WriteAttributeString("name", value.LabelName);

        _csfValueListXmlSerializer.Serialize(writer, value.Values);

        // </Label>
        writer.WriteEndElement();
    }
}