using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Shimakaze.Sdk.Extension.Server.Models.PIXI;

internal sealed record class Rect(int X, int Y, int W, int H)
{
    public static Rect Empty { get; } = new(0, 0, 0, 0);
}
internal sealed record class TextureBorders(int Left, int Top, int Right, int Bottom);
internal sealed record class PointData(int X, int Y);
internal sealed record class Size(int W, int H);
internal sealed record class SpriteSize(int X, int Y, int? W = default, int? H = default);

internal sealed record class SpritesheetFrameData(
    Rect Frame,
    bool? Trimmed = default,
    bool? Rotated = default,
    Size? SourceSize = default,
    SpriteSize? SpriteSourceSize = default,
    PointData? Anchor = default,
    TextureBorders? Borders = default
)
{
    public static SpritesheetFrameData Empty { get; } = new(Rect.Empty);
}

internal sealed record class FrameTag(int From, string Name, int To, string Direction);
internal sealed record class Layer(string BlendMode, string Name, float Opacity);
internal sealed record class Key(int Frame, Rect Bounds);
internal sealed record class Slice(string Color, string Name, Key[] Keys);

internal sealed record class SpritesheetMeta(
    string? App = default,
    string? Format = default,
    FrameTag[]? FrameTags = default,
    Layer[]? Layers = default,
    float Scale = 1,
    Size? Size = default,
    Slice[]? Slices = default,
    [property: JsonProperty("related_multi_packs")]
    [property: JsonPropertyName("related_multi_packs")]
    string[]? RelatedMultiPacks = default,
    string? Version = default
)
{
    public string? Image { get; set; }
}

internal sealed record class SpritesheetData(
    Dictionary<string, SpritesheetFrameData> Frames,
    SpritesheetMeta Meta,
    Dictionary<string, string[]>? Animations = default
);