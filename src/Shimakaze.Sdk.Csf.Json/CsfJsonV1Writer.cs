using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// CsfJsonV1Writer.
/// </summary>
public sealed class CsfJsonV1Writer(Stream stream, JsonSerializerOptions? options = null, bool leaveOpen = false) : ICsfWriter, IDisposable, IAsyncDisposable
{
    private readonly JsonSerializerOptions _options = options.Init(CsfJsonSerializerOptions.Converters);

    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public async Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
        => await JsonSerializer.SerializeAsync(_disposable, value, _options, cancellationToken);
}