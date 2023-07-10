using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.IO.Csf.Yaml;

/// <summary>
/// CSF YAML Serializer.
/// </summary>
public sealed class CsfYamlV1Writer : AsyncWriter<CsfDocument>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 构造器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public CsfYamlV1Writer(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        using StreamWriter writer = new(BaseStream, leaveOpen: true);
        new SerializerBuilder()
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Serialize(writer, value);
    }
}