using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// CSF YAML Deserializer.
/// </summary>
/// <remarks>
/// 构造器
/// </remarks>
/// <param name="reader"> 基础流 </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfYamlV1Reader(TextReader reader, bool leaveOpen = false) : AsyncTextReader<CsfDocument>(reader, leaveOpen), ICsfReader
{
    /// <inheritdoc />
    public override async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return new DeserializerBuilder()
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Deserialize<CsfDocument>(BaseReader);
    }
}