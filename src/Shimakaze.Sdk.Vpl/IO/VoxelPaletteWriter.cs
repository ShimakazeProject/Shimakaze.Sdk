using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;

/// <summary>
/// VoxelPaletteWriter
/// </summary>
public sealed class VoxelPaletteWriter : IWriter<VoxelPalette>
{
    private readonly BinaryWriter _writer;

    /// <summary>
    /// VoxelPaletteWriter
    /// </summary>
    public VoxelPaletteWriter(BinaryWriter writer)
    {
        _writer = writer;
    }
    /// <inheritdoc/>

    public void Write(in VoxelPalette value)
    {
        VoxelPaletteHeader head = value.Header;
        Palette pal = value.Palette;
        unsafe
        {
            _writer.Write(new Span<byte>((byte*)&head, sizeof(VoxelPaletteHeader)));
            _writer.Write(new Span<byte>((byte*)&pal, sizeof(Palette)));
        }
    }
}