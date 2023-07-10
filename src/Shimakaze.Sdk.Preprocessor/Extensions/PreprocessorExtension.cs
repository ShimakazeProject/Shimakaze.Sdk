using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Preprocessor.Commands;

namespace Shimakaze.Sdk.Preprocessor.Extensions;

/// <summary>
/// PreprocessorExtension
/// </summary>
public static class PreprocessorExtension
{
    /// <summary>
    /// AddCommand
    /// </summary>
    /// <typeparam name="TPreprocessorCommand"> </typeparam>
    /// <param name="services"> </param>
    /// <returns> </returns>
    public static IServiceCollection AddCommand<TPreprocessorCommand>(this IServiceCollection services)
        where TPreprocessorCommand : class, IPreprocessorCommand
        => services.AddTransient<IPreprocessorCommand, TPreprocessorCommand>();

    /// <summary>
    /// #if #elif #else #endif
    /// </summary>
    /// <param name="services"> </param>
    /// <returns> </returns>
    public static IServiceCollection AddConditionCommands(this IServiceCollection services) => services
        .AddTransient<IPreprocessorCommand, IfCommand>()
        .AddTransient<IPreprocessorCommand, ElifCommand>()
        .AddTransient<IPreprocessorCommand, ElseCommand>()
        .AddTransient<IPreprocessorCommand, EndIfCommand>();

    /// <summary>
    /// #define #undef
    /// </summary>
    /// <param name="services"> </param>
    /// <returns> </returns>
    public static IServiceCollection AddDefineCommands(this IServiceCollection services) => services
        .AddTransient<IPreprocessorCommand, DefineCommand>()
        .AddTransient<IPreprocessorCommand, UndefCommand>();

    /// <summary>
    /// AddPreprocessor
    /// </summary>
    /// <param name="service"> IServiceCollection </param>
    public static IServiceCollection AddPreprocessor(this IServiceCollection service) => service.AddPreprocessor(i => i);

    /// <summary>
    /// AddPreprocessor
    /// </summary>
    /// <param name="service"> IServiceCollection </param>
    /// <param name="varBuilder"> </param>
    public static IServiceCollection AddPreprocessor(
        this IServiceCollection service,
        Func<IPreprocessorVariablesBuilder, IPreprocessorVariablesBuilder> varBuilder
    ) => service
        .AddSingleton<IConditionParser, ConditionParser>()
        .AddScoped(provider => varBuilder(new PreprocessorVariablesBuilder()).Build())
        .AddScoped<IPreprocessor, Preprocessor>();

    /// <summary>
    /// #region #endregion
    /// </summary>
    /// <param name="services"> </param>
    /// <returns> </returns>
    public static IServiceCollection AddRegionCommands(this IServiceCollection services) => services
        .AddTransient<IPreprocessorCommand, RegionCommand>()
        .AddTransient<IPreprocessorCommand, EndregionCommand>();
}