using System.Collections.Immutable;

namespace Shimakaze.Sdk.Tmp;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// <seealso href="https://modenc.renegadeprojects.com/TMP"/>
/// </remarks>
/// <param name="Header"></param>
/// <param name="Offsets">
/// Absolute file offset to start of tile data.
/// If zero, then the tile is empty
/// </param>
/// <param name="Tiles"></param>
public sealed record class TemplateFile(
    TemplateFileHeader Header,
    ImmutableArray<uint> Offsets,
    ImmutableArray<TemplateTileCell> Tiles
)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="isometric"></param>
    /// <returns></returns>
    public static TemplateFile ReadFrom(Stream stream, bool isometric = true)
    {
        long zero = stream.Position;
        stream.Read(out TemplateFileHeader header);
        uint count = header.BlockWidth * header.BlockHeight;
        int tileSize = (int)(header.BlockImageWidth * header.BlockImageHeight);
        if (isometric)
            tileSize /= 2;

        uint[] offsets = GC.AllocateUninitializedArray<uint>((int)count);
        stream.Read(offsets);

        List<TemplateTileCell> tiles = new(offsets.Length);
        for (int i = 0; i < offsets.Length; i++)
        {
            stream.Seek(zero, SeekOrigin.Begin);
            stream.Seek(offsets[i], SeekOrigin.Current);

            stream.Read(out TemplateTileCellHeader tileHeader);
            byte[] tile = GC.AllocateUninitializedArray<byte>(tileSize);
            stream.ReadExactly(tile);
            byte[] height = GC.AllocateUninitializedArray<byte>(tileSize);
            stream.ReadExactly(height);
            byte[] extra = [];
            if (tileHeader.Flags.HasFlag(TemplateTileCellFlags.HasExtraData))
            {
                extra = GC.AllocateUninitializedArray<byte>((int)(tileHeader.ExtraWidth * tileHeader.ExtraHeight));
                stream.ReadExactly(extra);
            }
            tiles.Add(new(tileHeader, [.. tile], [.. height], [.. extra]));
        }

        return new(header, [.. offsets], [.. tiles]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="file"></param>
    public static void WriteTo(Stream stream, in TemplateFile file)
    {
        stream.Write(file.Header);
        stream.Write(file.Offsets.AsSpan());
        foreach (var tile in file.Tiles)
        {
            stream.Write(tile.Header);
            stream.Write(tile.Tile.AsSpan());
            stream.Write(tile.Height.AsSpan());
            stream.Write(tile.Extra.AsSpan());
        }
    }
}
