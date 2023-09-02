using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
    /// 反射当前应用域内所有程序集的预处理器指令集
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    public static IEnumerable<Command> AddAllCommands(this IServiceCollection services)
    {
        var types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(i => i.GetExportedTypes())
            .Where(i => !i.IsInterface && !i.IsAbstract && i.IsAssignableTo(typeof(ICommandSet)) && i != typeof(ICommandSet));

        foreach (var type in types)
            services.AddTransient(typeof(ICommandSet), type);

        return types.SelectMany(GetCommands);
    }

    /// <summary>
    /// 添加一个预处理器指令集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IEnumerable<Command> AddCommands<T>(this IServiceCollection services)
        where T : class
    {
        services.AddTransient(typeof(T));

        return GetCommands(typeof(T));
    }

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