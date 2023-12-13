using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Writer.
/// </summary>
/// <remarks>
/// 构造器
/// </remarks>
/// <param name="stream"> 基础流 </param>
/// <param name="settings"></param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfXmlV1Writer(TextWriter stream, XmlWriterSettings? settings = null, bool leaveOpen = false) : AsyncTextWriter<CsfDocument>(stream, leaveOpen), ICsfWriter
{

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(BaseWriter, settings);
        serializer.Serialize(xmlWriter, value);
    }
}