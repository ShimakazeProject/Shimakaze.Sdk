using System.Collections.Immutable;

using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// Options for JsonRPCHostedService
/// </summary>
public sealed record class JsonRPCHostedServiceOptions
{
    /// <summary>
    /// Targets
    /// </summary>
    public ImmutableArray<Target> Targets { get; set; }
    /// <summary>
    /// JsonRPC
    /// </summary>
    public JsonRpc JsonRpc { get; set; } = default!;
}