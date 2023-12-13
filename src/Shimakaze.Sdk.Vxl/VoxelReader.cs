using Shimakaze.Sdk;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelReader
/// </summary>
public sealed class VoxelReader : AsyncReader<VXLFile>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// PaletteReader
    /// </summary>

    public VoxelReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        if (!BaseStream.CanSeek)
            throw new NotSupportedException("This Stream cannot support Seek");
    }

    /// <inheritdoc />
    public override async Task<VXLFile> ReadAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        VXLFile voxel = new();
        BaseStream.Read(out voxel.InternalHeader);

        uint limbDataOffset = 34 + Palette.ColorCount * 3 + voxel.Header.NumSections * 28;

        await using (PaletteReader reader = new(BaseStream, true))
            voxel.Palette = await reader.ReadAsync(cancellationToken);

        voxel.SectionHeaders = new SectionHeader[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 * ((float)i / voxel.Header.NumSections));

            BaseStream.Read(out voxel.SectionHeaders[i]);
        }

        voxel.SectionTailers = new SectionTailer[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 * ((float)i / voxel.Header.NumSections + 1));

            BaseStream.Seek(limbDataOffset + voxel.Header.BodySize + i * 92, SeekOrigin.Begin);
            BaseStream.Read(out voxel.SectionTailers[i]);
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

            BaseStream.Seek(start, SeekOrigin.Begin);
            BaseStream.Read(voxel.SectionData[i].SpanStart);

            BaseStream.Seek(end, SeekOrigin.Begin);
            BaseStream.Read(voxel.SectionData[i].SpanEnd);

            for (int j = 0; j < n; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                voxel.SectionData[i].Voxel[j] = new();
                if (voxel.SectionData[i].SpanStart[j] is -1 && voxel.SectionData[i].SpanEnd[j] is -1)
                    continue;

                List<VoxelSpanSegment> sections = [];
                BaseStream.Seek(data + voxel.SectionData[i].SpanStart[j], SeekOrigin.Begin);
                for (byte z = 0; z < voxel.SectionTailers[i].Size.Z;)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    VoxelSpanSegment voxelSpanSegment = new()
                    {
                        SkipCount = (byte)BaseStream.ReadByte()
                    };
                    z += voxelSpanSegment.SkipCount;

                    voxelSpanSegment.NumVoxels = (byte)BaseStream.ReadByte();
                    z += voxelSpanSegment.NumVoxels;

                    // if (z + voxelSpanSegment.NumVoxels > voxel.SectionTailers[i].Size.Z) throw
                    // new OverflowException();

                    voxelSpanSegment.Voxels = new Voxel[voxelSpanSegment.NumVoxels];
                    if (voxelSpanSegment.NumVoxels is > 0)
                        BaseStream.Read(voxelSpanSegment.Voxels);

                    voxelSpanSegment.NumVoxels2 = (byte)BaseStream.ReadByte();
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