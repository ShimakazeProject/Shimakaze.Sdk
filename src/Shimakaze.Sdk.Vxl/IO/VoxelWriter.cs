using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl;

/// <summary>
/// VoxelWriter
/// </summary>
public sealed class VoxelWriter : IWriter<Voxel>
{
    private readonly BinaryWriter _writer;

    /// <summary>
    /// VoxelWriter
    /// </summary>
    public VoxelWriter(BinaryWriter writer)
    {
        _writer = writer;
    }
    /// <inheritdoc/>

    public void Write(in Voxel value)
    {
        VoxelHeader header = value.Header;
        Palette palette = value.Palette;
        unsafe
        {
            _writer.Write(new Span<byte>((byte*)&header, sizeof(VoxelHeader)));
            _writer.Write(new Span<byte>((byte*)&palette, sizeof(Palette)));

            fixed (LimbHeader* ptr = value.LimbHeads)
                _writer.Write(new Span<byte>((byte*)&ptr, value.LimbHeads.Length * sizeof(LimbHeader)));

            foreach (var body in value.LimbBodies)
            {
                foreach (var v in body.SpanStart)
                    _writer.Write(v);
                foreach (var v in body.SpanEnd)
                    _writer.Write(v);

                foreach (var span in body.Data)
                {
                    _writer.Write(span.SkipCount);
                    _writer.Write(span.NumVoxels);

                    fixed (SpanVoxel* ptr = span.Voxels)
                        _writer.Write(new Span<byte>((byte*)&ptr, span.Voxels.Length * sizeof(SpanVoxel)));

                    _writer.Write(span.NumVoxels2);
                }
            }

            fixed (LimbTailer* ptr = value.LimbTails)
                _writer.Write(new Span<byte>((byte*)&ptr, value.LimbTails.Length * sizeof(LimbTailer)));
        }
    }
}