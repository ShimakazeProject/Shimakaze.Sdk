using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.IO.Pal;
/// <summary>
/// PaletteReader
/// </summary>
public sealed class PaletteReader : IReader<Palette>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// PaletteReader
    /// </summary>

    public PaletteReader(Stream stream, bool leaveOpen = false)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_leaveOpen)
            _stream.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            await _stream.DisposeAsync();
    }

    /// <inheritdoc/>
    public Palette Read()
    {
        Palette palette = new();
        _stream.Read(palette.Colors);
        return palette;
    }
}
