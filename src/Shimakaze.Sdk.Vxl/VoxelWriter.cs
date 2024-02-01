using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vxl;

/// <summary>
/// VoxelWriter
/// </summary>
public sealed class VoxelWriter
{
    /// <summary>
    /// VXL写入器
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stream"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    public static void Write(VoxelFile value, Stream stream, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        uint limbDataOffset = 34 + Palette.DefaultColorCount * 3 + value.Header.NumSections * 28;

        stream.Write(value.Header);

        PaletteWriter.Write(value.Palette, stream, skipPreprocess: true);

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