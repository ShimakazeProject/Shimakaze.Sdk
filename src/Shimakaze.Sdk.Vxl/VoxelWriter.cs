using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelWriter
/// </summary>
/// <remarks>
/// PaletteReader
/// </remarks>
public sealed class VoxelWriter(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();


    /// <inheritdoc />
    public void Write(VoxelFile value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        uint limbDataOffset = 34 + Palette.ColorCount * 3 + value.Header.NumSections * 28;

        stream.Write(value.Header);

        using (PaletteWriter writer = new(stream, true))
            writer.Write(value.Palette);

        stream.Write(value.SectionHeaders);

        for (int i = 0; i < value.SectionData.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.SectionData.Length);

            long data = limbDataOffset + value.SectionTailers[i].SpanDataOffset;
            stream.Write(value.SectionData[i].SpanStart);
            stream.Write(value.SectionData[i].SpanEnd);

            for (int j = 0; j < value.SectionData[i].Voxel.Length; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                stream.Seek(data + value.SectionData[i].SpanStart[j], SeekOrigin.Begin);
                foreach (var span in value.SectionData[i].Voxel[j].Sections)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    stream.WriteByte(span.SkipCount);
                    stream.WriteByte(span.NumVoxels);

                    stream.Write(span.Voxels);

                    stream.WriteByte(span.NumVoxels2);
                }
            }
        }

        stream.Write(value.SectionTailers);
    }
}