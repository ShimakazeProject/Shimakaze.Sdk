using System.Collections.Immutable;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// JsonRPCHostedService
/// </summary>
public sealed class JsonRPCHostedService : IHostedService
{
    private readonly JsonRpc _jsonRpc;
    private readonly ImmutableArray<Target> _methods;
    private readonly IServiceProvider _provider;
    private readonly ILogger<JsonRPCHostedService>? _logger;

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public JsonRPCHostedService(JsonRPCHostedServiceOptions options, IServiceProvider serviceProvider, ILogger<JsonRPCHostedService>? logger = null)
    {
        _jsonRpc = options.JsonRpc;
        _methods = options.Targets;
        _provider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var method in _methods)
        {
            var isEvent = method.Method.ReturnType == typeof(void);
            _logger?.LogInformation("Find {type}: {path}", isEvent ? "Event" : "Method", method.Route);
            _jsonRpc.AddLocalRpcMethod(method.Route, method.Method, _provider.GetService(method.Type));
        }

        return Task.Run(_jsonRpc.StartListening, cancellationToken);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("See you next time!");
        return Task.CompletedTask;
    }
}
