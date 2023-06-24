using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.IO.Pal;
/// <summary>
/// PaletteReader
/// </summary>
public sealed class PaletteReader : IReader<Palette>
{
    private readonly byte[] _bytes;

    /// <summary>
    /// PaletteReader
    /// </summary>
    public PaletteReader(byte[] bytes)
    {
        _bytes = bytes;
    }

    /// <inheritdoc/>
    public Palette Read()
    {
        unsafe
        {
            fixed (byte* ptr = _bytes)
            {
                return *(Palette*)ptr;
            }
        }
    }
}
