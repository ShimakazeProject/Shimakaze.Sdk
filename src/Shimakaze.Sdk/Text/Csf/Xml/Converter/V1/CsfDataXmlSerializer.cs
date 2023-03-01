using System.Diagnostics;
using System.Xml;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Tools.Csf.Serialization.Xml.Converter.V1;

/// <summary>
/// Csf数据序列化器
/// </summary>
public class CsfDataXmlSerializer : IXmlSerializer<CsfData>
{
    private readonly CsfValueListXmlSerializer csfValueListXmlSerializer = new();
    private readonly CsfValueXmlSerializer csfValueXmlSerializer = new();

    /// <inheritdoc/>
    public CsfData Deserialize(XmlReader reader)
    {
        CsfData label = new();
        if (reader.NodeType is XmlNodeType.Element && reader.Name is "Label")
        {
            string? lbl = reader.GetAttribute("name");
            if (lbl is "GUI:Blank")
            {
                Debugger.Break();
            }

            if (!string.IsNullOrWhiteSpace(lbl))
            {
                label.LabelName = lbl;
            }

            label.Values = reader["extra"] switch
            {
                not null => new[] { csfValueXmlSerializer.Deserialize(reader) },
                _ => csfValueListXmlSerializer.Deserialize(reader),
            };
        }
        return label;
    }

    /// <inheritdoc/>
    public void Serialize(XmlWriter writer, CsfData value)
    {
        // <Label name="label_name">
        writer.WriteStartElement("Label");
        writer.WriteAttributeString("name", value.LabelName);

        csfValueListXmlSerializer.Serialize(writer, value.Values);

        // </Label>
        writer.WriteEndElement();
    }
}