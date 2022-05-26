namespace Shimakaze.Sdk.Models.Mix;

public sealed record class MixHeader(
    uint? Flag,
    short FileCount,
    int BodySize
);
