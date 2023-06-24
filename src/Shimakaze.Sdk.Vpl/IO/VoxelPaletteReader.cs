using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;
/// <summary>
/// VoxelPaletteReader
/// </summary>
public sealed class VoxelPaletteReader : IReader<VoxelPalette>, IAsyncReader<ValueTask<VoxelPalette>>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer = new byte[256];

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
        unsafe
        {
            _stream.Read(_buffer.AsSpan(0, sizeof(VoxelPaletteHeader)));
            fixed (byte* p = _buffer)
                vpl.Header = *(VoxelPaletteHeader*)p;

            using (PaletteReader reader = new(_stream, true))
                vpl.Palette = reader.Read();

            vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
            fixed (VoxelPaletteSection* p = vpl.Sections)
            fixed (byte* pBuf = _buffer)
            {
                for (int i = 0; i < vpl.Header.SectionCount; i++)
                {
                    _stream.Read(_buffer.AsSpan(0, sizeof(VoxelPaletteSection)));
                    p[i] = *(VoxelPaletteSection*)pBuf;
                }
            }

        }
        return vpl;
    }

    /// <inheritdoc/>
    public async ValueTask<VoxelPalette> ReadAsync(CancellationToken cancellationToken = default)
    {
        VoxelPalette vpl = new();
        await _stream.ReadAsync(_buffer.AsMemory(0, 16), cancellationToken);
        unsafe
        {
            fixed (byte* p = _buffer)
                vpl.Header = *(VoxelPaletteHeader*)p;
        }

        using (PaletteReader reader = new(_stream, true))
            vpl.Palette = await reader.ReadAsync(cancellationToken);

        vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
        for (int i = 0; i < vpl.Header.SectionCount; i++)
        {
            await _stream.ReadAsync(_buffer.AsMemory(0, 256), cancellationToken);
            unsafe
            {
                fixed (VoxelPaletteSection* p = vpl.Sections)
                fixed (byte* pBuf = _buffer)
                {
                    p[i] = *(VoxelPaletteSection*)pBuf;
                }
            }
        }
        return vpl;
    }
}
