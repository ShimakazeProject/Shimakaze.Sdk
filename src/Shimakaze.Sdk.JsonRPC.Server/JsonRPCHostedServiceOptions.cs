using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// Options for JsonRPCHostedService
/// </summary>
public sealed class JsonRPCHostedServiceOptions
{
    /// <summary>
    /// Input Stream
    /// </summary>
    public Stream? Input { get; set; }
    /// <summary>
    /// Output Stream
    /// </summary>
    public Stream? Output { get; set; }
    /// <summary>
    /// JsonRpcMessageTextFormatter
    /// </summary>
    public IJsonRpcMessageTextFormatter? JsonRpcMessageTextFormatter { get; set; }
}
