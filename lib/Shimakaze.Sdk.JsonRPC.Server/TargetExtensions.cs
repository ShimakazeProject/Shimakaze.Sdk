using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using System.Data;

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
    public static IServiceCollection AddJsonRpcService(this IServiceCollection services, Action<JsonRPCHostedServiceOptions, IServiceCollection> optionsBuilder)
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
                JsonRPCHostedServiceOptions options = new();
                optionsBuilder(options, services);
                return options;
            })
            .AddHostedService<JsonRPCHostedService>();
    }

    /// <summary>
    /// 添加整个应用域范围内的所有RpcHandler
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IEnumerable<Target> AddAllRpcHandlers(this IServiceCollection services)
    {
        return AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(i => i.ExportedTypes)
            .Select(i => (Type: i, Metadata: i.GetCustomAttribute<HandlerAttribute>()))
            .Where(i => i.Metadata is not null)
            .SelectMany(i => services.AddRpcHandler(i.Type, i.Metadata?.Route ?? TrimController(i.Type.Name)));
    }

    /// <summary>
    /// 添加一个RpcHandler
    /// </summary>
    /// <param name="services"></param>
    /// <param name="type"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    public static IEnumerable<Target> AddRpcHandler(this IServiceCollection services, Type type, string? route)
    {
        services.AddTransient(type);

        return GetRpcMethods(type, route);
    }

    /// <summary>
    /// 添加一个RpcHandler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    public static IEnumerable<Target> AddRpcHandler<T>(this IServiceCollection services, string? route) where T : class => services.AddRpcHandler(typeof(T), route);

    [GeneratedRegex("Handlers?$|Controllers?$")]
    private static partial Regex HandlerRegex();

    private static string Combine(string? s1, string? s2)
    {
        if (s1 is null && s2 is null)
            throw new ArgumentException("Must be set s1 or s2 ", nameof(s1));

        s1 ??= string.Empty;
        s2 ??= string.Empty;

        string result;
        if (s1.EndsWith('/') && s2.StartsWith('/'))
            result = s1[..^1] + s2;
        else if (!s1.EndsWith('/') && !s2.StartsWith('/'))
            result = s1 + '/' + s2;
        else
            result = s1 + s2;

        if (!result.StartsWith('/'))
            result = '/' + result;

        return result;
    }

    private static string TrimController(string path) => HandlerRegex().Replace(path, string.Empty);
    private static string TrimAsyncTail(string path)
    {
        if (path.EndsWith("Async"))
            return path[..^5];

        return path;
    }

    private static IEnumerable<Target> GetRpcMethods(Type type, string? route) => type
            .GetMethods()
            .Select(i => (Metadata: i.GetCustomAttribute<MethodAttribute>(), Method: i))
            .Where(i => i.Metadata is not null)
            .Select(i => new Target(Combine(route, i.Metadata!.Route ?? TrimAsyncTail(i.Method.Name)), i.Method, type));

}