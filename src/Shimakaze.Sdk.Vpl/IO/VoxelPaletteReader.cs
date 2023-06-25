using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;
/// <summary>
/// VoxelPaletteReader
/// </summary>
public sealed class VoxelPaletteReader : IReader<VoxelPalette>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// VoxelPaletteReader
    /// </summary>
    public VoxelPaletteReader(Stream stream, bool leaveOpen = false)
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
    public VoxelPalette Read()
    {
        VoxelPalette vpl = new();

        _stream.Read(out vpl.Header);

        using (PaletteReader reader = new(_stream, true))
            vpl.Palette = reader.Read();

        vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
        for (int i = 0; i < vpl.Sections.Length; i++)
        {
            vpl.Sections[i] = new();
            _stream.Read(vpl.Sections[i].Data);
        }

        return vpl;
    }
}
