using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf数据序列化器
/// </summary>
public class CsfLabelXmlSerializer : IXmlSerializer<CsfLabel>
{
    private readonly CsfValueListXmlSerializer _csfValueListXmlSerializer = new();
    private readonly CsfValueXmlSerializer _csfValueXmlSerializer = new();

    /// <inheritdoc />
    public CsfLabel Deserialize(XmlReader reader)
    {
        CsfLabel label = new(string.Empty);
        if (reader.NodeType is XmlNodeType.Element && reader.Name is "Label")
        {
            string? lbl = reader.GetAttribute("name");
            if (!string.IsNullOrWhiteSpace(lbl))
            {
                label.Name = lbl;
            }

            label.Values.AddRange(reader.GetAttribute("extra") switch
            {
                not null => [_csfValueXmlSerializer.Deserialize(reader)],
                _ => [.. _csfValueListXmlSerializer.Deserialize(reader)],
            });
        }
        return label;
    }

    /// <inheritdoc />
    public void Serialize(XmlWriter writer, CsfLabel value)
    {
        // <Label name="label_name">
        writer.WriteStartElement("Label");
        writer.WriteAttributeString("name", value.Name);

        _csfValueListXmlSerializer.Serialize(writer, value.Values);

        // </Label>
        writer.WriteEndElement();
    }
}
