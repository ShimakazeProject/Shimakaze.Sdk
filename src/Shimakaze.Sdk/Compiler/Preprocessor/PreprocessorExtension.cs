using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Compiler.Preprocessor.Commands;
using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Compiler.Preprocessor;

/// <summary>
/// PreprocessorExtension
/// </summary>
public static class PreprocessorExtension
{
    /// <summary>
    /// AddPreprocessor
    /// </summary>
    /// <param name="service">IServiceCollection</param>
    public static IServiceCollection AddPreprocessor(this IServiceCollection service) => service.AddPreprocessor(i => i);

    /// <summary>
    /// AddPreprocessor
    /// </summary>
    /// <param name="service">IServiceCollection</param>
    /// <param name="varBuilder"></param>
    public static IServiceCollection AddPreprocessor(
        this IServiceCollection service,
        Func<IPreprocessorVariablesBuilder, IPreprocessorVariablesBuilder> varBuilder
    ) => service
        .AddSingleton<IConditionParser, ConditionParser>()
        .AddScoped<IPreprocessorVariables>(provider => varBuilder(new PreprocessorVariablesBuilder(provider)).Build())
        .AddScoped<IPreprocessor, Preprocessor.Kernel.Preprocessor>();

    /// <summary>
    /// AddCommand
    /// </summary>
    /// <typeparam name="TPreprocessorCommand"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCommand<TPreprocessorCommand>(this IServiceCollection services)
        where TPreprocessorCommand : class, IPreprocessorCommand
        => services.AddTransient<IPreprocessorCommand, TPreprocessorCommand>();

    /// <summary>
    /// #region #endregion
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRegionCommands(this IServiceCollection services) => services
        .AddTransient<IPreprocessorCommand, RegionCommand>()
        .AddTransient<IPreprocessorCommand, EndregionCommand>();

    /// <summary>
    /// #if #elif #else #endif
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddConditionCommands(this IServiceCollection services) => services
        .AddTransient<IPreprocessorCommand, IfCommand>()
        .AddTransient<IPreprocessorCommand, ElifCommand>()
        .AddTransient<IPreprocessorCommand, ElseCommand>()
        .AddTransient<IPreprocessorCommand, EndIfCommand>();

    /// <summary>
    /// #define #undef
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefineCommands(this IServiceCollection services) => services
        .AddTransient<IPreprocessorCommand, DefineCommand>()
        .AddTransient<IPreprocessorCommand, UndefCommand>();
}

