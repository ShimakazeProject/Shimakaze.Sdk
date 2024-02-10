using Microsoft.Extensions.Hosting;

using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Shimakaze.Sdk.LanguageServer;

internal sealed partial class LanguageServerHostedService(
    ILanguageServer server
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await server.Initialize(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
