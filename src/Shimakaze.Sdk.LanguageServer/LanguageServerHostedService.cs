using System;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Shared;

namespace Shimakaze.Sdk.LanguageServer;

internal sealed partial class LanguageServerHostedService(
    IHostEnvironment environment,
    ILanguageServer server,
    ILogger<LanguageServerHostedService> logger
    ) : IHostedService
{
    private readonly ILogger<LanguageServerHostedService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment())
        {
            if (server is OmniSharp.Extensions.LanguageServer.Server.LanguageServer languageServer)
            {
                foreach (var descriptor in languageServer.HandlersManager.Descriptors)
                {
                    LogHandler(descriptor.Method, descriptor.ImplementationType);
                }
            }
        }

        await server.Initialize(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Debug, "发现处理程序: [{method}] {type}")]
    private partial void LogHandler(string method, Type type);
}
