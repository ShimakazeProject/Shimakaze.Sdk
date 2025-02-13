using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Reader.
/// </summary>
public static class CsfXmlV1Reader
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static CsfData Read(TextReader reader, XmlReaderSettings? settings = default)
    {
        CsfDataXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(reader, settings);
        return serializer.Deserialize(xmlReader);
    }
}
