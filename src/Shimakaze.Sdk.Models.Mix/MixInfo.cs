namespace Shimakaze.Sdk.Models.Mix;

public sealed record class MixInfo(
    MixHeader Header,
    MixIndexEntry[] Entries
);