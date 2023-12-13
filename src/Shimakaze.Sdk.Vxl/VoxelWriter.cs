using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelWriter
/// </summary>
/// <remarks>
/// PaletteReader
/// </remarks>
public sealed class VoxelWriter(Stream stream, bool leaveOpen = false) : AsyncWriter<VXLFile>(stream, leaveOpen), IDisposable, IAsyncDisposable
{

    /// <inheritdoc />
    public override async Task WriteAsync(VXLFile value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        uint limbDataOffset = 34 + Palette.ColorCount * 3 + value.Header.NumSections * 28;

        BaseStream.Write(value.Header);

        await using (PaletteWriter writer = new(BaseStream, true))
            await writer.WriteAsync(value.Palette, cancellationToken);

        BaseStream.Write(value.SectionHeaders);

        for (int i = 0; i < value.SectionData.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.SectionData.Length);

            long data = limbDataOffset + value.SectionTailers[i].SpanDataOffset;
            BaseStream.Write(value.SectionData[i].SpanStart);
            BaseStream.Write(value.SectionData[i].SpanEnd);

            for (int j = 0; j < value.SectionData[i].Voxel.Length; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                BaseStream.Seek(data + value.SectionData[i].SpanStart[j], SeekOrigin.Begin);
                foreach (var span in value.SectionData[i].Voxel[j].Sections)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    BaseStream.WriteByte(span.SkipCount);
                    BaseStream.WriteByte(span.NumVoxels);

                    BaseStream.Write(span.Voxels);

                    BaseStream.WriteByte(span.NumVoxels2);
                }
            }
        }

        BaseStream.Write(value.SectionTailers);
    }
}