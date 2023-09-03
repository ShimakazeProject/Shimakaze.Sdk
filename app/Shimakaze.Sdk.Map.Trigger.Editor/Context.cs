using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.Map.Trigger;

internal sealed class Context
{
    public required string Path { get; init; }
    public required IniDocument Ini { get; init; }
}
