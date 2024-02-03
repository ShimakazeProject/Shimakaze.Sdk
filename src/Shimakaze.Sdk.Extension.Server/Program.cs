using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json.Serialization;

using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.JsonRpc.Serialization;

using Shimakaze.Sdk.ShpViewer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<InitializeHostedService>();
builder.Services.AddJsonRpcServer(options =>
{
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardError())
        .WithSerializer(new JsonRpcSerializer()
        {
            Settings =
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        });

    foreach (var handlerType in typeof(Program)
                                        .Assembly
                                        .GetCustomAttributes<AssemblyJsonRpcHandlersAttribute>()
                                        .SelectMany(i => i.Types)
                                        .Distinct())
    {
        options.AddHandler(handlerType);
    }
});

var host = builder.Build();

host.Run();
