using System;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl;

/// <summary>
/// VoxelWriter
/// </summary>
public sealed class VoxelWriter : IWriter<VXLFile>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    /// <summary>
    /// PaletteReader
    /// </summary>

    public VoxelWriter(Stream stream, bool leaveOpen = false)
    {
        _stream = stream;
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
    public void Write(in VXLFile value)
    {
        uint limbDataOffset = 34 + Palette.COLOR_COUNT * 3 + value.Header.NumSections * 28;

        _stream.Write(value.Header);

        using (PaletteWriter writer = new(_stream, true))
            writer.Write(value.Palette);

        _stream.Write(value.SectionHeaders);

        for (int i = 0; i < value.SectionData.Length; i++)
        {
            long data = limbDataOffset + value.SectionTailers[i].SpanDataOffset;
            _stream.Write(value.SectionData[i].SpanStart);
            _stream.Write(value.SectionData[i].SpanEnd);

            for (int j = 0; j < value.SectionData[i].Voxel.Length; j++)
            {
                _stream.Seek(data + value.SectionData[i].SpanStart[j], SeekOrigin.Begin);
                foreach (var span in value.SectionData[i].Voxel[j].Sections)
                {
                    _stream.WriteByte(span.SkipCount);
                    _stream.WriteByte(span.NumVoxels);

                    _stream.Write(span.Voxels);

                    _stream.WriteByte(span.NumVoxels2);
                }
            }
        }

        _stream.Write(value.SectionTailers);
    }
}