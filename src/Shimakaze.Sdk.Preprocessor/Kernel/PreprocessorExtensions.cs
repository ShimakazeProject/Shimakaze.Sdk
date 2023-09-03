using System.Collections.Immutable;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器扩展方法
/// </summary>
public static class PreprocessorExtensions
{
    private static IEnumerable<Command> GetCommands(Type type) => type
            .GetMethods()
            .Select(i => (Metadata: i.GetCustomAttribute<CommandAttribute>(), Method: i))
            .Where(i => i.Metadata is not null)
            .Select(i => new Command(type, i.Metadata?.Name, i.Method, i.Method.GetCommandParameters().ToImmutableArray()));

    private static IEnumerable<CommandParameter> GetCommandParameters(this MethodInfo method) => method
        .GetParameters()
        .Select(i =>
        {
            string? regex = i.GetCustomAttribute<ParamAttribute>()?.Regex;
            if (string.IsNullOrEmpty(regex))
                regex = @"\w*";

            return new CommandParameter(regex, i);
        });

    /// <summary>
    /// 添加一个预处理器指令集
    /// </summary>
    /// <param name="services"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<Command> AddCommands(this IServiceCollection services, Type type)
    {
        services.AddTransient(type);

        return GetCommands(type);
    }

    /// <summary>
    /// 添加一个预处理器指令集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IEnumerable<Command> AddCommands<T>(this IServiceCollection services) where T : class => services.AddCommands(typeof(T));

    /// <summary>
    /// 添加引擎
    /// </summary>
    /// <param name="services"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IServiceCollection AddEngine(this IServiceCollection services, Action<EngineOptions, IServiceCollection> action)
    {
        EngineOptions options = new();
        action(options, services);
        return services
            .AddSingleton<Engine>()
            .AddSingleton(options);
    }
}