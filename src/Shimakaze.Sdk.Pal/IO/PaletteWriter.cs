using System.Runtime.InteropServices;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.IO.Pal;

/// <summary>
/// PaletteWriter
/// </summary>
public sealed class PaletteWriter : IWriter<Palette>, IAsyncWriter<Palette, ValueTask>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// PaletteWriter
    /// </summary>
    public PaletteWriter(Stream stream, bool leaveOpen = false)
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
    public void Write(in Palette value)
    {
        byte[] buffer = new byte[Palette.COLOR_COUNT * 3];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                Marshal.StructureToPtr(value, (nint)ptr, false);
                _stream.Write(buffer);
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(Palette value, CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[Palette.COLOR_COUNT * 3];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                Marshal.StructureToPtr(value, (nint)ptr, false);
            }
        }
        await _stream.WriteAsync(buffer, cancellationToken);
    }
}
