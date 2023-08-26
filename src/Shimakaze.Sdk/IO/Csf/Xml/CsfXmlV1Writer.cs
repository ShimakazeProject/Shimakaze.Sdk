using System.Xml;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.IO.Csf.Xml;

/// <summary>
/// CsfXmlV1Writer.
/// </summary>
public sealed class CsfXmlV1Writer : AsyncWriter<CsfDocument>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public CsfXmlV1Writer(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlWriter xmlWriter = XmlWriter.Create(BaseStream);
        serializer.Serialize(xmlWriter, value);
    }
}