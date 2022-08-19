
namespace Shimakaze.Sdk.Loader;

public class ReadOptions : IReadOptions
{
    public static IReadOptions Default { get; } = new ReadOptions();
}
public class ReadOptions<TReadOptions> : IReadOptions
    where TReadOptions : IReadOptions, new()
{
    public static TReadOptions Default { get; } = new();
}
