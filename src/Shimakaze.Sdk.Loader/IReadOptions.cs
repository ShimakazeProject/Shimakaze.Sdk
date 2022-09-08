
using System.Runtime.Versioning;

namespace Shimakaze.Sdk.Loader;

public interface IReadOptions
{
    [RequiresPreviewFeatures]
    public static abstract IReadOptions Default { get; }
}