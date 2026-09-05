using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VXL 文件
/// </summary>
public sealed record class VoxelFile(VoxelHeader Header, Palette Palette, Memory<SectionHeader> SectionHeaders, SectionData[] SectionData, Memory<SectionTailer> SectionTailers)
{
    /// <summary>
    /// 文件头
    /// </summary>
    public VoxelHeader Header { get; set; } = Header;
    /// <summary>
    /// 文件色板
    /// </summary>
    public Palette Palette { get; set; } = Palette;
    /// <summary>
    /// </summary>
    public Memory<SectionHeader> SectionHeaders { get; set; } = SectionHeaders;
    /// <summary>
    /// </summary>
    public SectionData[] SectionData { get; set; } = SectionData;
    /// <summary>
    /// </summary>
    public Memory<SectionTailer> SectionTailers { get; set; } = SectionTailers;

    /// <summary>
    /// VXL读取器
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static VoxelFile ReadFrom(Stream stream)
    {
        stream.Read(out VoxelHeader header);

        uint limbDataOffset = 34 + (Palette.DefaultColorCount * 3) + (header.NumSections * 28);

        var palette = Palette.ReadFrom(stream);

        Memory<SectionHeader> sectionHeaders = new SectionHeader[header.NumSections];
        stream.Read(sectionHeaders);

        Memory<SectionTailer> sectionTailers = new SectionTailer[header.NumSections];
        for (int i = 0; i < header.NumSections; i++)
        {
            stream.Seek(limbDataOffset + header.BodySize + (i * 92), SeekOrigin.Begin);
            stream.Read(out sectionTailers.Span[i]);
        }

        var sectionData = new SectionData[header.NumSections];
        for (int i = 0; i < header.NumSections; i++)
        {
            int n = sectionTailers.Span[i].Size.X * sectionTailers.Span[i].Size.Y;
            long start = limbDataOffset + sectionTailers.Span[i].SpanStartOffset;
            long end = limbDataOffset + sectionTailers.Span[i].SpanEndOffset;
            long data = limbDataOffset + sectionTailers.Span[i].SpanDataOffset;

            sectionData[i] = new(n);

            stream.Seek(start, SeekOrigin.Begin);
            stream.Read(sectionData[i].SpanStart);

            stream.Seek(end, SeekOrigin.Begin);
            stream.Read(sectionData[i].SpanEnd);

            for (int j = 0; j < n; j++)
            {
                if (sectionData[i].SpanStart.Span[j] is -1 && sectionData[i].SpanEnd.Span[j] is -1)
                    continue;

                stream.Seek(data + sectionData[i].SpanStart.Span[j], SeekOrigin.Begin);

                List<VoxelSpanSegment> sections = [];
                for (byte z = 0; z < sectionTailers.Span[i].Size.Z;)
                {
                    var voxelSpanSegment = VoxelSpanSegment.ReadFrom(stream);
                    z += voxelSpanSegment.SkipCount;
                    z += voxelSpanSegment.NumVoxels;

                    // if (z + voxelSpanSegment.NumVoxels > sectionTailers.Span[i].Size.Z) throw
                    // new OverflowException();

                    sections.Add(voxelSpanSegment);
                }

                sectionData[i].Voxel[j] = new([.. sections]);
            }
        }
        return new(header, palette, sectionHeaders, sectionData, sectionTailers);
    }
    /// <summary>
    /// VXL写入器
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stream"></param>
    public static void WriteTo(VoxelFile value, Stream stream)
    {
        uint limbDataOffset = 34 + (Palette.DefaultColorCount * 3) + (value.Header.NumSections * 28);

        stream.Write(value.Header);

        value.Palette.WriteTo(stream);

        stream.Write((ReadOnlyMemory<SectionHeader>)value.SectionHeaders);

        for (int i = 0; i < value.SectionData.Length; i++)
        {
            long data = limbDataOffset + value.SectionTailers.Span[i].SpanDataOffset;
            stream.Write((ReadOnlyMemory<int>)value.SectionData[i].SpanStart);
            stream.Write((ReadOnlyMemory<int>)value.SectionData[i].SpanEnd);

            for (int j = 0; j < value.SectionData[i].Voxel.Length; j++)
            {
                stream.Seek(data + value.SectionData[i].SpanStart.Span[j], SeekOrigin.Begin);
                foreach (var span in value.SectionData[i].Voxel[j].Sections)
                    span.WriteTo(stream);
            }
        }

        stream.Write(value.SectionTailers.Span);
    }
}
