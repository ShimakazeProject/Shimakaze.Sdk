using System.Xml;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Tools.Csf.Serialization.Xml.Converter.V1;

/// <summary>
/// Csf数据列表序列化器
/// </summary>
public class CsfDataListXmlSerializer : IXmlSerializer<IList<CsfData>>
{
    private readonly CsfDataXmlSerializer csfDataXmlSerializer = new();

    /// <inheritdoc/>
    public IList<CsfData> Deserialize(XmlReader reader)
    {
        List<CsfData> data = new();
        while (reader.Read())
        {
            if (reader.NodeType is XmlNodeType.Whitespace)
            {
                continue;
            }

            if (reader.NodeType is XmlNodeType.Element && reader.Name is "Label")
            {
                data.Add(csfDataXmlSerializer.Deserialize(reader));
            }
            else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Resources")
            {
                break;
            }
        }
        return data;
    }

    /// <inheritdoc/>
    public void Serialize(XmlWriter writer, IList<CsfData> value)
    {
        foreach (var item in value)
        {
            csfDataXmlSerializer.Serialize(writer, item);
        }
    }
}