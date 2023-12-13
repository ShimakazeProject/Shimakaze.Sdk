using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Writer.
/// </summary>
/// <remarks>
/// ������
/// </remarks>
/// <param name="stream"> ������ </param>
/// <param name="settings"></param>
/// <param name="leaveOpen"> �˳�ʱ�Ƿ񱣳����� </param>
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