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
    public static readonly XmlWriterSettings DefaultSettings = new()
    {
        Indent = true,
        IndentChars = "  ",
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="settings"></param>
    public static void Write(TextWriter writer, CsfData value, XmlWriterSettings? settings = default)
    {
        settings ??= DefaultSettings;
        CsfDataXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(writer, settings);
        serializer.Serialize(xmlWriter, value);
    }
}
