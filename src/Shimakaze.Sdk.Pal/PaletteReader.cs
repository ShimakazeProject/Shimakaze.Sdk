namespace Shimakaze.Sdk.Pal;

/// <summary>
/// PaletteReader
/// </summary>
public sealed class PaletteReader(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public Palette Read()
    {
        Palette palette = new();
        _disposable.Resource.Read(palette.Colors);
        return palette;
    }
}