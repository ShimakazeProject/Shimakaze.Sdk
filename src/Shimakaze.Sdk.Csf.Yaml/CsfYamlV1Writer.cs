using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// CSF YAML Serializer.
/// </summary>
/// <remarks>
/// ������
/// </remarks>
/// <param name="stream"> ������ </param>
/// <param name="leaveOpen"> �˳�ʱ�Ƿ񱣳����� </param>
public sealed class CsfYamlV1Writer(TextWriter stream, bool leaveOpen = false) : AsyncTextWriter<CsfDocument>(stream, leaveOpen), ICsfWriter
{

    /// <inheritdoc />
    public override async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        new SerializerBuilder()
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Serialize(BaseWriter, value);
    }
}