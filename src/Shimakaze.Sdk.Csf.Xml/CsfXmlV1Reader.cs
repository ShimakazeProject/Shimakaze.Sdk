using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Reader.
/// </summary>
/// <param name="reader"> 基础流 </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfXmlV1Reader(TextReader reader, XmlReaderSettings? settings = null, bool leaveOpen = false) : ICsfReader, IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<TextReader> _disposable = new(reader, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();


    /// <inheritdoc />
    public async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(_disposable, settings);
        return serializer.Deserialize(xmlReader);
    }
}