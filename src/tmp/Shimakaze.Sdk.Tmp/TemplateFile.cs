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
);
