using System;

using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl;
/// <summary>
/// VoxelReader
/// </summary>
public sealed class VoxelReader : IReader<VXLFile>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// PaletteReader
    /// </summary>

    public VoxelReader(Stream stream, bool leaveOpen = false)
    {
        _stream = stream;
        if (!_stream.CanSeek)
            throw new NotSupportedException("This Stream cannot support Seek");
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
    public VXLFile Read()
    {
        VXLFile voxel = new();
        _stream.Read(out voxel.Header);

        uint limbDataOffset = 34 + Palette.COLOR_COUNT * 3 + voxel.Header.NumSections * 28;

        using (PaletteReader reader = new(_stream, true))
            voxel.Palette = reader.Read();

        voxel.SectionHeaders = new SectionHeader[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
            _stream.Read(out voxel.SectionHeaders[i]);

        voxel.SectionTailers = new SectionTailer[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
            _stream.Seek(limbDataOffset + voxel.Header.BodySize + i * 92, SeekOrigin.Begin);
            _stream.Read(out voxel.SectionTailers[i]);
        }

        voxel.SectionData = new SectionData[voxel.Header.NumSections];
        for (int i = 0; i < voxel.Header.NumSections; i++)
        {
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

            _stream.Seek(start, SeekOrigin.Begin);
            _stream.Read(voxel.SectionData[i].SpanStart);

            _stream.Seek(end, SeekOrigin.Begin);
            _stream.Read(voxel.SectionData[i].SpanEnd);

            for (int j = 0; j < n; j++)
            {
                voxel.SectionData[i].Voxel[j] = new();
                if (voxel.SectionData[i].SpanStart[j] is -1 && voxel.SectionData[i].SpanEnd[j] is -1)
                    continue;

                List<VoxelSpanSegment> sections = new();
                _stream.Seek(data + voxel.SectionData[i].SpanStart[j], SeekOrigin.Begin);
                for (byte z = 0; z < voxel.SectionTailers[i].Size.Z;)
                {
                    VoxelSpanSegment voxelSpanSegment = new()
                    {
                        SkipCount = (byte)_stream.ReadByte()
                    };
                    z += voxelSpanSegment.SkipCount;

                    voxelSpanSegment.NumVoxels = (byte)_stream.ReadByte();
                    z += voxelSpanSegment.NumVoxels;

                    // if (z + voxelSpanSegment.NumVoxels > voxel.SectionTailers[i].Size.Z)
                    //     throw new OverflowException();

                    voxelSpanSegment.Voxels = new Voxel[voxelSpanSegment.NumVoxels];
                    if (voxelSpanSegment.NumVoxels is > 0)
                        _stream.Read(voxelSpanSegment.Voxels);

                    voxelSpanSegment.NumVoxels2 = (byte)_stream.ReadByte();
                    if (voxelSpanSegment.NumVoxels != voxelSpanSegment.NumVoxels2)
                        throw new FormatException("NumVoxels are not equal than NumVoxels2");

                    sections.Add(voxelSpanSegment);
                }

                voxel.SectionData[i].Voxel[j].Sections = sections.ToArray();
            }
        }
        return voxel;
    }
}