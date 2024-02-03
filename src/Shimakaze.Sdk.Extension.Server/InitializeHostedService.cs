using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.JsonRpc;

namespace Shimakaze.Sdk.ShpViewer
{
    internal sealed partial class InitializeHostedService(
        JsonRpcServer server,
        ILogger<InitializeHostedService> logger) : IHostedService
    {
        private readonly ILogger _logger = logger;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var handler in server.HandlersManager.GetHandlers())
            {
                FindHandler(handler.GetType().GetCustomAttribute<MethodAttribute>()?.Method, handler.GetType());
            }

            return server.Initialize(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        [LoggerMessage(LogLevel.Information, "Find Handler: [{method}] {type}")]
        private partial void FindHandler(string? method, Type type);
    }
}
