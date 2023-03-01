using System.Xml;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Tools.Csf.Serialization.Xml.Converter.V1;

/// <summary>
/// Csf值转换器
/// </summary>
public class CsfValueListXmlSerializer : IXmlSerializer<IList<CsfValue>>
{
    private readonly CsfValueXmlSerializer csfValueXmlSerializer = new();

    /// <inheritdoc/>
    public IList<CsfValue> Deserialize(XmlReader reader)
    {
        List<CsfValue> values = new();
        while (reader.Read())
        {
            if (reader.NodeType is XmlNodeType.Whitespace)
            {
                continue;
            }

            if (reader.NodeType is XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Value":
                        values.Add(csfValueXmlSerializer.Deserialize(reader));
                        break;

                    case "Values":
                        while (reader.Read())
                        {
                            if (reader.NodeType is XmlNodeType.Whitespace)
                            {
                                continue;
                            }

                            if (reader.NodeType is XmlNodeType.Element && reader.Name is "Value")
                            {
                                values.Add(csfValueXmlSerializer.Deserialize(reader));
                            }
                            else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Values")
                            {
                                break;
                            }
                        }
                        break;
                }
            }
            else if (reader.NodeType is XmlNodeType.Text)
            {
                values.Add(csfValueXmlSerializer.Deserialize(reader));
            }
            else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Label")
            {
                break;
            }
        }

        if (values.Count is 0)
        {
            values.Add(new(string.Empty));
        }

        return values.ToArray();
    }

    /// <inheritdoc/>
    public void Serialize(XmlWriter writer, IList<CsfValue> value)
    {
        if (value.Count != 1)
        {
            // <Values>
            writer.WriteStartElement("Values");
            foreach (var item in value)
            {
                // <Value>
                writer.WriteStartElement("Value");
                csfValueXmlSerializer.Serialize(writer, item);

                // </Value>
                writer.WriteEndElement();
            }

            // </Values>
            writer.WriteEndElement();
        }
        else
        {
            csfValueXmlSerializer.Serialize(writer, value[0]);
        }
    }
}