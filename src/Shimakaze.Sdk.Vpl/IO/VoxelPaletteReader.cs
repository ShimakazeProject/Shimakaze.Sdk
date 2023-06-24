using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl;
/// <summary>
/// VoxelPaletteReader
/// </summary>
public sealed class VoxelPaletteReader : IReader<VoxelPalette>
{
    private readonly byte[] _bytes;

    /// <summary>
    /// VoxelPaletteReader
    /// </summary>
    public VoxelPaletteReader(byte[] bytes)
    {
        _bytes = bytes;
    }

    /// <inheritdoc/>
    public VoxelPalette Read()
    {
        unsafe
        {
            fixed (byte* p = _bytes)
            {
                byte* ptr = p;
                VoxelPalette vpl = new()
                {
                    Header = *(VoxelPaletteHeader*)ptr
                };
                ptr += sizeof(VoxelPaletteHeader);

                vpl.Palette = *(Palette*)ptr;
                ptr += sizeof(Palette);

                vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
                fixed (VoxelPaletteSection* pSections = vpl.Sections)
                {
                    Buffer.MemoryCopy(ptr, pSections, sizeof(VoxelPaletteSection) * vpl.Header.SectionCount, sizeof(VoxelPaletteSection) * vpl.Header.SectionCount);
                }
                return vpl;
            }
        }
    }
}
