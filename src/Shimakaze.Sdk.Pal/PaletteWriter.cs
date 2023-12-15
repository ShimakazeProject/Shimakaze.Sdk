namespace Shimakaze.Sdk.Pal;

/// <summary>
/// PaletteWriter
/// </summary>
public sealed class PaletteWriter(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <summary>
    /// 写入调色板
    /// </summary>
    /// <param name="value"></param>
    public void Write(in Palette value)
    {
        _disposable.Resource.Write(value.Colors);
    }
}