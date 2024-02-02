using System.Collections.Immutable;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// JsonRPCHostedService
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="options"></param>
/// <param name="serviceProvider"></param>
/// <param name="logger"></param>
public sealed partial class JsonRPCHostedService(JsonRPCHostedServiceOptions options, IServiceProvider serviceProvider, ILogger<JsonRPCHostedService>? logger = null) : IHostedService
{
    private readonly JsonRpc _jsonRpc = options.JsonRpc;
    private readonly ImmutableArray<Target> _methods = options.Targets;

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var method in _methods)
        {
            var isEvent = method.Method.ReturnType == typeof(void);
            if (logger is not null)
                Find(logger, isEvent ? "Event" : "Method", method.Route);
            _jsonRpc.AddLocalRpcMethod(method.Route, method.Method, serviceProvider.GetService(method.Type));
        }

        return Task.Run(_jsonRpc.StartListening, cancellationToken);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (logger is not null)
            Bye(logger);
        return Task.CompletedTask;
    }

    [LoggerMessage("Find {Type}: {Path}")]
    private static partial void Find(ILogger logger, string type, string path, LogLevel level = LogLevel.Debug);
    [LoggerMessage("See you next time!")]
    private static partial void Bye(ILogger logger, LogLevel level = LogLevel.Information);
}