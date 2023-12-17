using System.Xml;

using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// CSF YAML Serializer.
/// </summary>
/// <param name="writer"> 基础流 </param>
/// <param name="builder"></param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfYamlV1Writer(TextWriter writer, Func<SerializerBuilder, SerializerBuilder>? builder = null, bool leaveOpen = false) : ICsfWriter, IDisposable, IAsyncDisposable
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

        builder ??= i => i;

        builder(new())
           .WithTypeConverter(CsfValueConverter.Instance)
           .WithTypeConverter(CsfDataConverter.Instance)
           .WithTypeConverter(CsfDocumentConverter.Instance)
           .Build()
           .Serialize(_disposable, value);
    }
}