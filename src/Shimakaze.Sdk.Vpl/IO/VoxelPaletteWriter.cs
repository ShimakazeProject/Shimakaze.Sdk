using System.Runtime.InteropServices;

using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;

/// <summary>
/// VoxelPaletteWriter
/// </summary>
public sealed class VoxelPaletteWriter : IWriter<VoxelPalette>, IAsyncWriter<VoxelPalette, ValueTask>, IDisposable, IAsyncDisposable
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
        byte[] buffer = new byte[256];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                Marshal.StructureToPtr(value.Header, (nint)ptr, false);
                _stream.Write(buffer.AsSpan(0, sizeof(VoxelPaletteHeader)));
            }

            using (PaletteWriter writer = new(_stream, true))
                writer.Write(value.Palette);

            int size = value.Sections.Length * sizeof(VoxelPaletteSection);
            fixed (VoxelPaletteSection* p = value.Sections)
                _stream.Write(new Span<byte>(p, size));
        }
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(VoxelPalette value, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[16];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                Marshal.StructureToPtr(value.Header, (nint)ptr, false);
            }
        }

        await _stream.WriteAsync(buffer.AsMemory(0, 16), cancellationToken);

        using (PaletteWriter writer = new(_stream, true))
            await writer.WriteAsync(value.Palette, cancellationToken);

        int size = value.Sections.Length * 256;
        Memory<byte> memory;
        unsafe
        {
            fixed (VoxelPaletteSection* p = value.Sections)
                memory = new Span<byte>(p, size).ToArray();
        }
        await _stream.WriteAsync(memory, cancellationToken);
    }
}