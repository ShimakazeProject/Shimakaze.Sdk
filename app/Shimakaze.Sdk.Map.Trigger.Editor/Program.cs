using System.Collections.Immutable;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Nerdbank.Streams;

using Shimakaze.Sdk.JsonRPC.Server;


await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddJsonConsole();
    })
    .ConfigureServices(services => services.AddJsonRpcService((options, services) =>
    {
        options.Targets = services.AddAllRpcHandlers().ToImmutableArray();
        options.JsonRpc = new(FullDuplexStream.Splice(Console.OpenStandardInput(), Console.OpenStandardError()));
    }))
    .RunConsoleAsync();
