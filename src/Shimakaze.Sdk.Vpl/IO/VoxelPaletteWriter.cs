using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;

/// <summary>
/// VoxelPaletteWriter
/// </summary>
public sealed class VoxelPaletteWriter : IWriter<VoxelPalette>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// VoxelPaletteWriter
    /// </summary>
    public VoxelPaletteWriter(Stream stream, bool leaveOpen = false)
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
    public void Write(in VoxelPalette value)
    {
        _stream.Write(value.Header);

        using (PaletteWriter writer = new(_stream, true))
            writer.Write(value.Palette);

        for (int i = 0; i < value.Sections.Length; i++)
            _stream.Write<byte>(value.Sections[i].Data);
    }
}