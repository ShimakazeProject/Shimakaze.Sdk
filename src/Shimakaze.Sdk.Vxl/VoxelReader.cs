using Shimakaze.Sdk;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelReader
/// </summary>
public sealed class VoxelReader(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream.CanSeek(), leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public VXLFile Read(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        VXLFile voxel = new();
        stream.Read(out voxel.InternalHeader);

        uint limbDataOffset = 34 + Palette.ColorCount * 3 + voxel.Header.NumSections * 28;

        using (PaletteReader reader = new(stream, true))
            voxel.Palette = reader.Read();

        voxel.SectionHeaders = new SectionHeader[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 * ((float)i / voxel.Header.NumSections));

            stream.Read(out voxel.SectionHeaders[i]);
        }

        voxel.SectionTailers = new SectionTailer[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 * ((float)i / voxel.Header.NumSections + 1));

            stream.Seek(limbDataOffset + voxel.Header.BodySize + i * 92, SeekOrigin.Begin);
            stream.Read(out voxel.SectionTailers[i]);
        }

        voxel.SectionData = new SectionData[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 * ((float)i / voxel.Header.NumSections + 2));

            int n = voxel.SectionTailers[i].Size.X * voxel.SectionTailers[i].Size.Y;
            long start = limbDataOffset + voxel.SectionTailers[i].SpanStartOffset;
            long end = limbDataOffset + voxel.SectionTailers[i].SpanEndOffset;
            long data = limbDataOffset + voxel.SectionTailers[i].SpanDataOffset;

            voxel.SectionData[i] = new()
            {
                SpanStart = new int[n],
                SpanEnd = new int[n],
                Voxel = new VoxelSpan[n],
            };

            stream.Seek(start, SeekOrigin.Begin);
            stream.Read(voxel.SectionData[i].SpanStart);

            stream.Seek(end, SeekOrigin.Begin);
            stream.Read(voxel.SectionData[i].SpanEnd);

            for (int j = 0; j < n; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                voxel.SectionData[i].Voxel[j] = new();
                if (voxel.SectionData[i].SpanStart[j] is -1 && voxel.SectionData[i].SpanEnd[j] is -1)
                    continue;

                List<VoxelSpanSegment> sections = [];
                stream.Seek(data + voxel.SectionData[i].SpanStart[j], SeekOrigin.Begin);
                for (byte z = 0; z < voxel.SectionTailers[i].Size.Z;)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    VoxelSpanSegment voxelSpanSegment = new()
                    {
                        SkipCount = (byte)stream.ReadByte()
                    };
                    z += voxelSpanSegment.SkipCount;

                    voxelSpanSegment.NumVoxels = (byte)stream.ReadByte();
                    z += voxelSpanSegment.NumVoxels;

                    // if (z + voxelSpanSegment.NumVoxels > voxel.SectionTailers[i].Size.Z) throw
                    // new OverflowException();

                    voxelSpanSegment.Voxels = new Voxel[voxelSpanSegment.NumVoxels];
                    if (voxelSpanSegment.NumVoxels is > 0)
                        stream.Read(voxelSpanSegment.Voxels);

                    voxelSpanSegment.NumVoxels2 = (byte)stream.ReadByte();
                    if (voxelSpanSegment.NumVoxels != voxelSpanSegment.NumVoxels2)
                        throw new FormatException("NumVoxels are not equal than NumVoxels2");

                    sections.Add(voxelSpanSegment);
                }

                voxel.SectionData[i].Voxel[j].Sections = [.. sections];
            }
        }
        return voxel;
    }
}