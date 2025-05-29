using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter.V1;

/// <summary>
/// Csf数据列表序列化器
/// </summary>
public class CsfLabelListXmlSerializer : IXmlSerializer<IList<CsfLabel>>
{
    private readonly CsfLabelXmlSerializer _csfLabelXmlSerializer = new();

    /// <inheritdoc />
    public IList<CsfLabel> Deserialize(XmlReader reader)
    {
        List<CsfLabel> data = [];
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element when reader.Name is "Label":
                    data.Add(_csfLabelXmlSerializer.Deserialize(reader));
                    break;

                    // case XmlNodeType.EndElement when reader.Name is "Resources": goto outer;
            }
        }
        // outer:
        return data;
    }

    /// <inheritdoc />
    public void Serialize(XmlWriter writer, IList<CsfLabel> value)
    {
        foreach (CsfLabel item in value)
        {
            _csfLabelXmlSerializer.Serialize(writer, item);
        }
    }
}
