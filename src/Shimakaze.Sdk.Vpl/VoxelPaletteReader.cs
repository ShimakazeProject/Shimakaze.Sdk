using Shimakaze.Sdk;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// VoxelPaletteReader
/// </summary>
/// <remarks>
/// VoxelPaletteReader
/// </remarks>
public sealed class VoxelPaletteReader(Stream stream, bool leaveOpen = false) : AsyncReader<VoxelPalette>(stream, leaveOpen), IDisposable, IAsyncDisposable
{

    /// <inheritdoc />
    public override async Task<VoxelPalette> ReadAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        VoxelPalette vpl = new();

        BaseStream.Read(out vpl.InternalHeader);

        await using (PaletteReader reader = new(BaseStream, true))
            vpl.Palette = await reader.ReadAsync(cancellationToken);

        vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
        for (int i = 0; i < vpl.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / vpl.Sections.Length);

            vpl.Sections[i] = new();
            unsafe
            {
                fixed (byte* p = vpl.Sections[i].Data)
                    BaseStream.Read(new Span<byte>(p, 256));
            }
        }

        return vpl;
    }
}