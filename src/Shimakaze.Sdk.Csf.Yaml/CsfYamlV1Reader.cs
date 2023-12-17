using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// CSF YAML Deserializer.
/// </summary>
/// <param name="reader"> 基础流 </param>
/// <param name="builder"></param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfYamlV1Reader(TextReader reader, Func<DeserializerBuilder, DeserializerBuilder>? builder = null, bool leaveOpen = false) : ICsfReader, IDisposable, IAsyncDisposable
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

        builder ??= i => i;

        return builder(new())
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Deserialize<CsfDocument>(_disposable);
    }
}