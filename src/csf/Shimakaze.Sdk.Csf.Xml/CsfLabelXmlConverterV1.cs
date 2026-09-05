using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// Csf数据序列化器
/// </summary>
public sealed class CsfLabelXmlConverterV1 : XmlConverter<CsfLabel>
{
    /// <inheritdoc/>
    public override CsfLabel Read(XmlReader reader, XmlSerializerOptions options)
    {
        CsfLabel label = new(reader.GetAttribute("name") ?? string.Empty);
        string? extra = reader.GetAttribute("extra");

        if (!options.TryGetConverter<CsfValueXmlConverterV1>(out var converter))
            converter = new();

        if (reader.IsEmptyElement)
        {
            reader.Read();
            return label;
        }

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    if (!string.IsNullOrEmpty(reader.Value))
                        label.Add(reader.Value, extra);
                    break;

                case XmlNodeType.Element when reader.Name is "Value":
                    label.Add(converter.Read(reader, options));
                    break;

                case XmlNodeType.Element when reader.Name is "Values":
                    ReadValues(reader, label, converter, options);
                    break;

                case XmlNodeType.EndElement when reader.Name is "Label":
                    return label;
            }
        }

        return label;
    }

    private static void ReadValues(XmlReader reader, CsfLabel label, CsfValueXmlConverterV1 converter, XmlSerializerOptions options)
    {
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return;
        }

        while (reader.Read())
        {
            if (reader.NodeType is XmlNodeType.Element && reader.Name is "Value")
                label.Add(converter.Read(reader, options));
            else if (reader.NodeType is XmlNodeType.EndElement && reader.Name is "Values")
                break;
        }
    }

    /// <inheritdoc/>
    public override void Write(XmlWriter writer, CsfLabel value, XmlSerializerOptions options)
    {
        writer.WriteStartElement("Label");
        writer.WriteAttributeString("name", value.Name);

        if (!options.TryGetConverter<CsfValueXmlConverterV1>(out var converter))
            converter = new();

        switch (value.Count)
        {
            case 1:
                WriteSingle(writer, value[0]);
                break;

            case > 1:
                writer.WriteStartElement("Values");

                foreach (var item in value)
                    converter.Write(writer, item, options);

                writer.WriteEndElement();
                break;
        }

        writer.WriteEndElement();
    }

    private static void WriteSingle(XmlWriter writer, CsfValue value)
    {
        writer.WriteStartElement("Label");

        if (value.Extra is not null)
            writer.WriteAttributeString("extra", value.Extra);

        writer.WriteString(value.Value);

        writer.WriteEndElement();
    }
}
