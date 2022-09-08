
using System.Runtime.Versioning;

namespace Shimakaze.Sdk.Loader;

public class ReadOptions : IReadOptions
{
    [RequiresPreviewFeatures]
    public static IReadOptions Default { get; } = new ReadOptions();
}
public class ReadOptions<TReadOptions> : IReadOptions
    where TReadOptions : IReadOptions, new()
{
    public static TReadOptions Default { get; } = new();

    [RequiresPreviewFeatures]
    static IReadOptions IReadOptions.Default => ReadOptions<TReadOptions>.Default;
}
