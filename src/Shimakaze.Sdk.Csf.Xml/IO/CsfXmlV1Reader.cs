using System.Xml;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.IO.Csf.Xml;

/// <summary>
/// CsfXmlV1Reader.
/// </summary>
public sealed class CsfXmlV1Reader : AsyncReader<CsfDocument>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public CsfXmlV1Reader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(BaseStream);
        return serializer.Deserialize(xmlReader);
    }
}