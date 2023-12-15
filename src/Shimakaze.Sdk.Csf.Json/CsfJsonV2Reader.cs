using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V2;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// CsfJsonV2Reader.
/// </summary>
public sealed class CsfJsonV2Reader(Stream stream, JsonSerializerOptions? options = null, bool leaveOpen = false) : ICsfReader, IDisposable, IAsyncDisposable
{
    private readonly JsonSerializerOptions _options = options.Init(CsfJsonSerializerOptions.Converters);

    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public async Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        CsfDocument? csf = await JsonSerializer.DeserializeAsync<CsfDocument>(_disposable, _options, cancellationToken);
        CsfJsonAsserts.IsNotNull(csf);
        return csf;
    }
}