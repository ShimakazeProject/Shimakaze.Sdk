using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf数据列表序列化器
/// </summary>
public class CsfDataListXmlSerializer : IXmlSerializer<IList<CsfData>>
{
    private readonly CsfDataXmlSerializer _csfDataXmlSerializer = new();

    /// <inheritdoc />
    public IList<CsfData> Deserialize(XmlReader reader)
    {
        List<CsfData> data = [];
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.Name is "Label":
                    data.Add(_csfDataXmlSerializer.Deserialize(reader));
                    break;

                    // case XmlNodeType.EndElement when reader.Name is "Resources": goto outer;
            }
        }
        // outer:
        return data;
    }

    /// <inheritdoc />
    public void Serialize(XmlWriter writer, IList<CsfData> value)
    {
        foreach (var item in value)
            _csfDataXmlSerializer.Serialize(writer, item);
    }
}