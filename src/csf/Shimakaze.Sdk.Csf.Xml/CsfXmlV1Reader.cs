using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Reader.
/// </summary>
public static class CsfXmlV1Reader
{
    /// <summary>
    /// 从文本流读取 CSF 数据
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static CsfData Read(TextReader reader, XmlSerializerOptions? options = default)
    {
        options ??= new XmlSerializerOptions();

        if (!options.TryGetConverter<CsfDataXmlConverterV1>(out var converter))
            converter = new();
        using var xmlReader = XmlReader.Create(reader, options.ReaderSettings);
        return converter.Read(xmlReader, options);
    }

    /// <summary>
    /// 从文本流读取 CSF 数据
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static CsfData Read(Stream stream, XmlSerializerOptions? options = default)
    {
        using StreamReader reader = new(stream, leaveOpen: true);
        return Read(reader, options);
    }
}
