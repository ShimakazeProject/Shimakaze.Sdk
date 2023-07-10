using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;

/// <summary>
/// VoxelPaletteWriter
/// </summary>
public sealed class VoxelPaletteWriter : AsyncWriter<VoxelPalette>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// VoxelPaletteWriter
    /// </summary>
    public VoxelPaletteWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

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

            BaseStream.Write<byte>(value.Sections[i].Data);
        }
    }
}