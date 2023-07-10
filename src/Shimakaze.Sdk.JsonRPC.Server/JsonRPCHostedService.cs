using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

internal sealed class JsonRPCHostedService : IHostedService, IDisposable
{
    private readonly JsonRpc _jsonRpc;
    private readonly ILogger<JsonRPCHostedService> _logger;
    private readonly JsonRPCHostedServiceOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceCollection _services;

    public JsonRPCHostedService(
        IServiceCollection services,
        IServiceProvider serviceProvider)
    {
        _services = services;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<JsonRPCHostedService>>();
        _options = serviceProvider.GetRequiredService<JsonRPCHostedServiceOptions>();
        _jsonRpc = new(_options.JsonRpcMessageHandler);
    }

    public void Dispose()
    {
        _jsonRpc.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        int count = 0;
        foreach (var target in _services
            .Select(i => (
                Metadata: i.ImplementationType?.GetCustomAttribute<HandlerAttribute>(),
                i.ImplementationType))
            .Where(i => i is
            {
                Metadata: not null,
                ImplementationType:
                {
                    IsInterface: false,
                    IsAbstract: false
                }
            })
            .SelectMany(i => i.ImplementationType!.GetTargets(i.Metadata?.Route, _serviceProvider.GetRequiredService(i.ImplementationType!))))
        {
            var isEvent = target.Method.ReturnType == typeof(void);
            _logger.LogDebug("Find {type}: {path}", isEvent ? "Event" : "Method", target.Path);
            _jsonRpc.AddLocalRpcMethod(target.Path, target.Method, target.Object);
            count++;
        }
        _logger.LogInformation("Loaded {count} JsonRPC Methods/Events.", count);
        _jsonRpc.StartListening();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("See you next time!");
        return Task.CompletedTask;
    }
}