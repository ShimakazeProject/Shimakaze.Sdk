namespace Shimakaze.Sdk.Hva;

/// <summary>
/// HvaWriter
/// </summary>
public sealed class HvaWriter(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public async Task WriteAsync(HvaFile value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        _disposable.Resource.Write(value.Header);
        _disposable.Resource.Write(value.SectionNames);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(1f / 3);

        await Task.Yield();

        for (int i = 0; i < value.Frames.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 + 2f / 3 * ((float)i / value.Frames.Length));

            HvaFrame item = value.Frames[i];
            _disposable.Resource.Write(item.Matrices);
        }
    }
}