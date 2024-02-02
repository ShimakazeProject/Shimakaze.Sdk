using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

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
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    public static void Write(TextWriter writer, CsfDocument value, XmlWriterSettings? settings = default, CancellationToken cancellationToken = default)
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(writer, settings);
        serializer.Serialize(xmlWriter, value);
    }
}