using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Writer.
/// </summary>
public static class CsfXmlV1Writer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public static void Write(TextWriter writer, CsfData value, XmlSerializerOptions? options = default)
    {
        options ??= new XmlSerializerOptions();

        if (!options.TryGetConverter<CsfDataXmlConverterV1>(out var converter))
            converter = new();
        using var xmlWriter = XmlWriter.Create(writer, options.WriterSettings);
        converter.Write(xmlWriter, value, options);
    }

    /// <summary>
    /// 写入到文本流
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public static void Write(Stream stream, CsfData value, XmlSerializerOptions? options = default)
    {
        using StreamWriter writer = new(stream, leaveOpen: true);
        Write(writer, value, options);
    }
}
