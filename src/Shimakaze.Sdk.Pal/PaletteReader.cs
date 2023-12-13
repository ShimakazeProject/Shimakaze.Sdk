namespace Shimakaze.Sdk.Pal;

/// <summary>
/// PaletteReader
/// </summary>
public sealed class PaletteReader : AsyncReader<Palette>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// PaletteReader
    /// </summary>
    public PaletteReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public Palette Read()
    {
        Palette palette = new();
        BaseStream.Read(palette.Colors);
        return palette;
    }

    /// <inheritdoc />
    public override async Task<Palette> ReadAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return Read();
    }
}