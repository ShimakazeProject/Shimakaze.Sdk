using System;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl;

/// <summary>
/// VoxelWriter
/// </summary>
public sealed class VoxelWriter : IWriter<Voxel>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private byte[] _buffer = new byte[1024];

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
    public void Write(in Voxel value)
    {
        unsafe
        {
            byte[] buffer = new byte[sizeof(VoxelHeader)];
            fixed (byte* ptr = buffer)
                Marshal.StructureToPtr(value.Header, (nint)ptr, false);
            _stream.Write(buffer.AsSpan(0, sizeof(VoxelHeader)));

            using (PaletteWriter writer = new(_stream, true))
                writer.Write(value.Palette);

            int size = value.LimbHeads.Length * sizeof(LimbHeader);
            fixed (LimbHeader* p = value.LimbHeads)
                _stream.Write(new Span<byte>(p, size));

            foreach (var body in value.LimbBodies)
            {
                size = body.SpanStart.Length * sizeof(int);

                fixed (int* p = body.SpanStart)
                    _stream.Write(new Span<byte>(p, size));

                fixed (int* p = body.SpanEnd)
                    _stream.Write(new Span<byte>(p, size));

                foreach (var span in body.Data)
                {
                    _stream.WriteByte(span.SkipCount);
                    _stream.WriteByte(span.NumVoxels);

                    size = span.Voxels.Length * sizeof(SpanVoxel);
                    fixed (SpanVoxel* p = span.Voxels)
                        _stream.Write(new Span<byte>(p, size));

                    _stream.WriteByte(span.NumVoxels2);
                }
            }

            size = value.LimbTails.Length * sizeof(LimbTailer);
            fixed (LimbTailer* p = value.LimbTails)
                _stream.Write(new Span<byte>(p, size));
        }
    }
}