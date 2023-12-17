using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf值转换器
/// </summary>
public class CsfValueListXmlSerializer : IXmlSerializer<IList<CsfValue>>
{
    private readonly CsfValueXmlSerializer _csfValueXmlSerializer = new();

    /// <inheritdoc />
    public IList<CsfValue> Deserialize(XmlReader reader)
    {
        List<CsfValue> values = [];
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.Name is "Value":
                    values.Add(_csfValueXmlSerializer.Deserialize(reader));
                    break;

                case XmlNodeType.Element when reader.Name is "Values":
                    while (reader.Read())
                    {
                        if (reader.NodeType is XmlNodeType.Element && reader.Name is "Value")
                            values.Add(_csfValueXmlSerializer.Deserialize(reader));
                        else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Values")
                            break;
                    }
                    break;

                case XmlNodeType.Text:
                    values.Add(_csfValueXmlSerializer.Deserialize(reader));
                    break;

                case XmlNodeType.EndElement when reader.Name is "Label":
                    goto outer;
            }
        }

    outer:
        if (values.Count is 0)
            values.Add(new(string.Empty));

        return values.ToArray();
    }

    /// <inheritdoc />
    public void Serialize(XmlWriter writer, IList<CsfValue> value)
    {
        if (value.Count is not 1)
        {
            // <Values>
            writer.WriteStartElement("Values");
            foreach (var item in value)
            {
                // <Value>
                writer.WriteStartElement("Value");
                _csfValueXmlSerializer.Serialize(writer, item);

                // </Value>
                writer.WriteEndElement();
            }

            // </Values>
            writer.WriteEndElement();
        }
        else
        {
            _csfValueXmlSerializer.Serialize(writer, value[0]);
        }
    }
}