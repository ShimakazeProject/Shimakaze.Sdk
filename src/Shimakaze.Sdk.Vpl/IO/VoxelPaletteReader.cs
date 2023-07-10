using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;

/// <summary>
/// VoxelPaletteReader
/// </summary>
public sealed class VoxelPaletteReader : AsyncReader<VoxelPalette>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// VoxelPaletteReader
    /// </summary>
    public VoxelPaletteReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task<VoxelPalette> ReadAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        VoxelPalette vpl = new();

        BaseStream.Read(out vpl.Header);

        await using (PaletteReader reader = new(BaseStream, true))
            vpl.Palette = await reader.ReadAsync(cancellationToken);

        vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
        for (int i = 0; i < vpl.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / vpl.Sections.Length);

            vpl.Sections[i] = new();
            BaseStream.Read(vpl.Sections[i].Data);
        }

        return vpl;
    }
}