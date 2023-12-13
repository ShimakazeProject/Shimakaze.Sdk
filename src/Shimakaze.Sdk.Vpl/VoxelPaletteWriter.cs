using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VoxelPaletteWriter
/// </summary>
/// <remarks>
/// VoxelPaletteWriter
/// </remarks>
public sealed class VoxelPaletteWriter(Stream stream, bool leaveOpen = false) : AsyncWriter<VoxelPalette>(stream, leaveOpen), IDisposable, IAsyncDisposable
{
    /// <inheritdoc />
    public override async Task WriteAsync(VoxelPalette value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        BaseStream.Write(value.Header);

        await using (PaletteWriter writer = new(BaseStream, true))
            await writer.WriteAsync(value.Palette, cancellationToken);

        for (int i = 0; i < value.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.Sections.Length);

            unsafe
            {
                fixed (byte* ptr = value.Sections[i].Data)
                    BaseStream.Write(new Span<byte>(ptr, 256));
            }
        }
    }
}