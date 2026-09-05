using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// XML序列化选项
/// </summary>
public sealed class XmlSerializerOptions
{
    /// <summary>
    /// 获取 XML 转换器集合
    /// </summary>
    public IList<XmlConverter> Converters { get; } =
    [
        new CsfDataXmlConverterV1(),
        new CsfLabelXmlConverterV1(),
        new CsfValueXmlConverterV1(),
    ];

    /// <summary>
    /// 
    /// </summary>
    public XmlReaderSettings? ReaderSettings { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public XmlWriterSettings? WriterSettings { get; set; } = new()
    {
        Indent = true,
        IndentChars = "  ",
    };
}
