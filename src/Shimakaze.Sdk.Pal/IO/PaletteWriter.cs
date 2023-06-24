using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.IO.Pal;

/// <summary>
/// PaletteWriter
/// </summary>
public sealed class PaletteWriter : IWriter<Palette>
{
    private readonly BinaryWriter _writer;

    /// <summary>
    /// PaletteWriter
    /// </summary>
    public PaletteWriter(BinaryWriter writer)
    {
        _writer = writer;
    }
    /// <inheritdoc/>

    public void Write(in Palette value)
    {
        unsafe
        {
            Palette pal = value;
            _writer.Write(new Span<byte>((byte*)&pal, sizeof(Palette)));
        }
    }
}
