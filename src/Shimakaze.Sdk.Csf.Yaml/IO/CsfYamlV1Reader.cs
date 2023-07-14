using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.IO.Csf.Yaml;

/// <summary>
/// CSF YAML Deserializer.
/// </summary>
public sealed class CsfYamlV1Reader : AsyncReader<CsfDocument>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public CsfYamlV1Reader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        using StreamReader reader = new(BaseStream, leaveOpen: true);
        return new DeserializerBuilder()
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Deserialize<CsfDocument>(reader);
    }
}