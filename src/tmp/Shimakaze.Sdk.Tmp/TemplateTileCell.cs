using System.Collections.Immutable;

namespace Shimakaze.Sdk.Tmp;

/// <summary>
/// 
/// </summary>
/// <param name="Header"></param>
/// <param name="Tile"></param>
/// <param name="Height"></param>
/// <param name="Extra"></param>
public sealed record class TemplateTileCell(
    TemplateTileCellHeader Header,
    ImmutableArray<byte> Tile,
    ImmutableArray<byte> Height,
    ImmutableArray<byte> Extra
);
