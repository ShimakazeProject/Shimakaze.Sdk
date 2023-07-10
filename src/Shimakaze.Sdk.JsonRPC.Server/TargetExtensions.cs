using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

namespace Shimakaze.Sdk.JsonRPC.Server;

/// <summary>
/// JsonRpc Server Helper
/// </summary>
public static partial class TargetExtensions
{
    /// <summary>
    /// 添加所有的Handler
    /// </summary>
    /// <param name="services"> </param>
    /// <param name="optionsBuilder"> </param>
    /// <returns> </returns>
    public static IServiceCollection AddJsonRpcHandlers(this IServiceCollection services, Action<JsonRPCHostedServiceOptions> optionsBuilder)
    {
        foreach (var t in AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => t.GetCustomAttribute<HandlerAttribute>() is not null))
            services.AddTransient(t, t);

        return services
            .AddSingleton(provider =>
            {
                JsonRPCHostedServiceOptions options;
                optionsBuilder(options = new() { JsonRpcMessageHandler = default! });
                return options.JsonRpcMessageHandler is null ? throw new NullReferenceException() : options;
            })
            .AddHostedService<JsonRPCHostedService>(provider => new(services, provider));
    }

    internal static IEnumerable<Target> GetTargets(this Type type, string? route, object? o)
    {
        return type
            .GetMethods()
            .Select(m => (metadata: m.GetCustomAttribute<MethodAttribute>(), m))
            .Where(i => i.metadata is not null)
            .Select(i => new Target(type.GetFullPath(route, i.metadata?.Route, i.m), i.m, o));
    }

    private static string GetFullPath(this Type type, string? route, string? method, MethodInfo m)
    {
        route ??= HandlerRegex().Replace(type.Name, string.Empty);
        method ??= m.Name;
        if (method.StartsWith('/'))
            return method;

        StringBuilder sb = new();
        if (!route.StartsWith('/'))
            sb.Append('/');
        sb.Append(route);
        if (!route.EndsWith('/'))
            sb.Append('/');

        sb.Append(method);
        return sb.ToString();
    }

    [GeneratedRegex("Handlers?|Controllers?")]
    private static partial Regex HandlerRegex();
}