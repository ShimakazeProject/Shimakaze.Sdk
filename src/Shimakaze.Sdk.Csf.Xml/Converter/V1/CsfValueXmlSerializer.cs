using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf值序列化器
/// </summary>
public class CsfValueXmlSerializer : IXmlSerializer<CsfValue>
{
    /// <inheritdoc/>
    public CsfValue Deserialize(XmlReader reader)
    {
        string? extra = null;
        string value = string.Empty;

        if (reader.NodeType is XmlNodeType.Text)
            value += reader.Value;
        else
        {
            if (reader.NodeType is XmlNodeType.Element)
                extra = reader.GetAttribute("extra");

            while (reader.Read())
            {
                if (reader.NodeType is XmlNodeType.Text)
                    value += reader.Value;
                else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Value" or "Label")
                    break;
            }
        }
        return string.IsNullOrWhiteSpace(extra) ? new CsfValue(value) : new CsfValueExtra(value, extra);
    }

    /// <inheritdoc/>
    public void Serialize(XmlWriter writer, CsfValue value)
    {
        if (value is CsfValueExtra extra)
            writer.WriteAttributeString("extra", extra.ExtraValue);

        writer.WriteString(value.Value);
    }
}