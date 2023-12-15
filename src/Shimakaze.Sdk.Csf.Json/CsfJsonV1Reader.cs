using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// CsfJsonV1Reader.
/// </summary>
/// <param name="stream"> 基础流 </param>
/// <param name="options"> </param>
/// <param name="leaveOpen"> 退出时是否保持流打开 </param>
public sealed class CsfJsonV1Reader(Stream stream, JsonSerializerOptions? options = null, bool leaveOpen = false) : ICsfReader, IDisposable, IAsyncDisposable
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