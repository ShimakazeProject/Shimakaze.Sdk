using System.Xml;

using Shimakaze.Sdk.Csf.Xml.Converter.V1;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// CsfXmlV1Reader.
/// </summary>
/// <remarks>
/// 构造器
/// </remarks>
/// <param name="stream"> 基础流 </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfXmlV1Reader(TextReader stream, bool leaveOpen = false) : AsyncTextReader<CsfDocument>(stream, leaveOpen), ICsfReader
{

    /// <inheritdoc />
    public override async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        CsfDocumentXmlSerializer serializer = new();
        using XmlReader xmlReader = XmlReader.Create(BaseReader);
        return serializer.Deserialize(xmlReader);
    }
}