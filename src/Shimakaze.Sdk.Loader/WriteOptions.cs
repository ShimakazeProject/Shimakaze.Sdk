
namespace Shimakaze.Sdk.Loader;

public class WriteOptions : IWriteOptions
{
    public static IWriteOptions Default { get; } = new WriteOptions();
}
public class WriteOptions<TWriteOptions> : IWriteOptions
    where TWriteOptions : IWriteOptions, new()
{
    public static TWriteOptions Default { get; } = new ();
}
