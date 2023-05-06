using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StreamJsonRpc;

namespace Shimakaze.Sdk.JsonRPC.Server;

internal sealed class JsonRPCHostedService : IHostedService
{
    private readonly IServiceCollection _services;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JsonRPCHostedService> _logger;
    private readonly JsonRPCHostedServiceOptions _options;

    public JsonRPCHostedService(
        IServiceCollection services,
        IServiceProvider serviceProvider)
    {
        _services = services;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<JsonRPCHostedService>>();
        _options = serviceProvider.GetRequiredService<JsonRPCHostedServiceOptions>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        JsonRpc jsonRpc = new(_options.JsonRpcMessageHandler ?? throw new ArgumentNullException(nameof(_options.JsonRpcMessageHandler)));
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
            jsonRpc.AddLocalRpcMethod(target.Path, target.Method, target.Object);
            count++;
        }
        _logger.LogInformation("Loaded {count} JsonRPC Methods/Events.", count);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}