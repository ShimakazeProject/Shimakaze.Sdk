using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.IO.Pal;
/// <summary>
/// PaletteReader
/// </summary>
public sealed class PaletteReader : IReader<Palette>, IAsyncReader<ValueTask<Palette>>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer = new byte[Palette.COLOR_COUNT * 3];

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
        _stream.Read(_buffer);
        unsafe
        {
            fixed (byte* ptr = _buffer)
                return *(Palette*)ptr;
        }
    }

    /// <inheritdoc/>
    public async ValueTask<Palette> ReadAsync(CancellationToken cancellationToken = default)
    {
        await _stream.ReadAsync(_buffer, cancellationToken);
        unsafe
        {
            fixed (byte* ptr = _buffer)
                return *(Palette*)ptr;
        }
    }
}
