using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.Csf.Xml.Serialization;

/// <summary>
/// CsfXmlV1Deserializer.
/// </summary>
public sealed class CsfXmlV1Deserializer :
    IDeserializer<CsfDocument>,
    IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    private readonly bool _leaveOpen;

    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    public CsfXmlV1Deserializer(Stream baseStream, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <inheritdoc/>
    public CsfDocument Deserialize()
    {
        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(BaseStream);
        return serializer.Deserialize(xmlReader);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                    BaseStream.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// 异步释放核心
    /// </summary>
    /// <returns></returns>
    private async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }

    // ~CsfSerializer()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
    }
}
