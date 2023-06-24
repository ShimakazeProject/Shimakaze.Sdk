using System;

using Shimakaze.Sdk.IO.Pal;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl;
/// <summary>
/// VoxelReader
/// </summary>
public sealed class VoxelReader : IReader<Voxel>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private byte[] _buffer = new byte[1024];

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
    public Voxel Read()
    {
        Voxel voxel = new();
        unsafe
        {
            _stream.Read(_buffer.AsSpan(0, sizeof(VoxelHeader)));
            fixed (byte* ptr = _buffer)
                voxel.Header = *(VoxelHeader*)ptr;

            int limbDataOffset = sizeof(VoxelHeader) + sizeof(Palette) + voxel.Header.LimbsCount * sizeof(LimbHeader);

            using (PaletteReader reader = new(_stream, true))
                voxel.Palette = reader.Read();


            voxel.LimbHeads = new LimbHeader[voxel.Header.LimbsCount];
            fixed (LimbHeader* p = voxel.LimbHeads)
            fixed (byte* ptr = _buffer)
            {
                for (int i = 0; i < voxel.Header.LimbsCount; i++)
                {
                    _stream.Read(_buffer.AsSpan(0, sizeof(LimbHeader)));
                    Buffer.MemoryCopy(ptr, p + i, sizeof(LimbHeader), sizeof(LimbHeader));
                }
            }

            voxel.LimbTails = new LimbTailer[voxel.Header.LimbsCount];
            fixed (LimbTailer* p = voxel.LimbTails)
            fixed (byte* ptr = _buffer)
            {
                for (int i = 0; i < voxel.Header.LimbsCount; i++)
                {
                    _stream.Seek(limbDataOffset + voxel.Header.BodySize + i * sizeof(LimbTailer), SeekOrigin.Begin);
                    _stream.Read(_buffer.AsSpan(0, sizeof(LimbTailer)));
                    Buffer.MemoryCopy(ptr, p + i, sizeof(LimbTailer), sizeof(LimbTailer));
                }
            }

            voxel.LimbBodies = new LimbBody[voxel.Header.LimbsCount];
            for (int i = 0; i < voxel.Header.LimbsCount; i++)
            {
                int n = voxel.LimbTails[i].Size.X * voxel.LimbTails[i].Size.Y;
                long start = limbDataOffset + voxel.LimbTails[i].SpanStartOffset;
                long end = limbDataOffset + voxel.LimbTails[i].SpanEndOffset;
                long data = limbDataOffset + voxel.LimbTails[i].SpanDataOffset;
                int size = n * sizeof(int);
                if (_buffer.Length < size)
                    _buffer = new byte[size];

                voxel.LimbBodies[i] = new()
                {
                    SpanStart = new int[n],
                    SpanEnd = new int[n],
                    Data = new SpanStruct[n],
                };

                fixed (int* p1 = voxel.LimbBodies[i].SpanStart)
                fixed (int* p2 = voxel.LimbBodies[i].SpanEnd)
                fixed (byte* ptr = _buffer)
                {
                    _stream.Seek(start, SeekOrigin.Begin);
                    _stream.Read(_buffer.AsSpan(0, size));
                    Buffer.MemoryCopy(ptr, p1, size, size);

                    _stream.Seek(end, SeekOrigin.Begin);
                    _stream.Read(_buffer.AsSpan(0, size));
                    Buffer.MemoryCopy(ptr, p2, size, size);
                }

                for (int j = 0; j < n; j++)
                {
                    _stream.Seek(data + voxel.LimbBodies[i].SpanStart[j], SeekOrigin.Begin);
                    voxel.LimbBodies[i].Data[j].SkipCount = (byte)_stream.ReadByte();
                    voxel.LimbBodies[i].Data[j].NumVoxels = (byte)_stream.ReadByte();
                    voxel.LimbBodies[i].Data[j].Voxels = new SpanVoxel[voxel.LimbBodies[i].Data[j].NumVoxels];
                    size = voxel.LimbBodies[i].Data[j].NumVoxels * sizeof(SpanVoxel);
                    if (_buffer.Length < size)
                        _buffer = new byte[size];
                    fixed (SpanVoxel* p = voxel.LimbBodies[i].Data[j].Voxels)
                    fixed (byte* ptr = _buffer)
                    {
                        _stream.Read(_buffer.AsSpan(0, size));
                        Buffer.MemoryCopy(ptr, p, size, size);
                    }

                    voxel.LimbBodies[i].Data[j].NumVoxels2 = (byte)_stream.ReadByte();
                    if (_stream.Position < data + voxel.LimbBodies[i].SpanStart[j])
                    {
                        // 少读内容了
                    }
                }
            }
        }
        return voxel;
    }
}
