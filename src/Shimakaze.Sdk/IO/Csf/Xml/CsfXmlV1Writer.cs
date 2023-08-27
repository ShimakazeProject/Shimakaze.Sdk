using System.Xml;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.IO.Csf.Xml;

/// <summary>
/// CsfXmlV1Writer.
/// </summary>
public sealed class CsfXmlV1Writer : AsyncWriter<CsfDocument>, IDisposable, IAsyncDisposable
{
    private readonly XmlWriterSettings? _settings;
    /// <summary>
    /// ������
    /// </summary>
    /// <param name="stream"> ������ </param>
    /// <param name="settings"></param>
    /// <param name="leaveOpen"> �˳�ʱ�Ƿ񱣳����� </param>
    public CsfXmlV1Writer(Stream stream, XmlWriterSettings? settings = null, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        _settings = settings;
    }

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(BaseStream, _settings);
        serializer.Serialize(xmlWriter, value);
    }
}