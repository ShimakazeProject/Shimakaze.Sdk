using System.Collections.Immutable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.JsonRPC.Server;
using Shimakaze.Sdk.Map.Trigger;

using StreamJsonRpc;

await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        // logging.AddJsonConsole();
        logging.AddSimpleConsole();
    })
    .ConfigureServices(services => services
        .AddSingleton<Context>()
        .AddJsonRpcService((options, services) =>
        {
            options.Targets = services.AddAllRpcHandlers().ToImmutableArray();
            options.JsonRpc = new(
                new NewLineDelimitedMessageHandler(
                    Console.OpenStandardError(),
                    Console.OpenStandardInput(),
                    new JsonMessageFormatter()
                )
            );
        })
    )
    .RunConsoleAsync();
