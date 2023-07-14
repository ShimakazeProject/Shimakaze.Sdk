using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// Options for JsonRPCHostedService
/// </summary>
public sealed class JsonRPCHostedServiceOptions
{
    /// <summary>
    /// JsonRpcMessageHandler
    /// </summary>
    public required IJsonRpcMessageHandler JsonRpcMessageHandler { get; set; }
}