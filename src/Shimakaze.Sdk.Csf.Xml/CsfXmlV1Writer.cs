using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Writer.
/// </summary>
/// <param name="writer"> ������ </param>
/// <param name="settings"></param>
/// <param name="leaveOpen"> �˳�ʱ�Ƿ񱣳����� </param>
public sealed class CsfXmlV1Writer(TextWriter writer, XmlWriterSettings? settings = null, bool leaveOpen = false) : ICsfWriter, IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<TextWriter> _disposable = new(writer, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(_disposable, settings);
        serializer.Serialize(xmlWriter, value);
    }
}