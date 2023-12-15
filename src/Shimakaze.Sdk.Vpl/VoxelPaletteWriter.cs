using Shimakaze.Sdk.Graphic.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VoxelPaletteWriter
/// </summary>
/// <remarks>
/// VoxelPaletteWriter
/// </remarks>
public sealed class VoxelPaletteWriter(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public void Write(VoxelPalette value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        _disposable.Resource.Write(value.Header);

        using (PaletteWriter writer = new(_disposable, true))
            writer.Write(value.Palette);

        for (int i = 0; i < value.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.Sections.Length);

            unsafe
            {
                fixed (byte* ptr = value.Sections[i].Data)
                    _disposable.Resource.Write(new Span<byte>(ptr, 256));
            }
        }
    }
}