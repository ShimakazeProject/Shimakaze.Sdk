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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static CsfDocument Read(TextReader reader, XmlReaderSettings? settings = default, CancellationToken cancellationToken = default)
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(reader, settings);
        return serializer.Deserialize(xmlReader);
    }
}