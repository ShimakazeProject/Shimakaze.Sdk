using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;
/// <summary>
/// Define Commands: define undef
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="engine"></param>
/// <param name="logger"></param>
public sealed class DefineCommand(Engine engine, Logger<DefineCommand>? logger = null)
{
    private static readonly Action<ILogger, string, Exception> LogDefine = LoggerMessage.Define<string>(LogLevel.Debug, 0, "Define {Identifier}");
    private static readonly Action<ILogger, string, Exception> LogUndefine = LoggerMessage.Define<string>(LogLevel.Debug, 0, "Undefine {Identifier}");

    /// <summary>
    /// #define identifier
    /// </summary>
    /// <param name="identifier"></param>
    [Command]
    public void Define(string identifier)
    {
        var defines = engine.GetOrNew("Defines", () => new HashSet<string>());
        defines.Add(identifier);
        if (logger is not null)
            LogDefine(logger, identifier, default!);
    }

    /// <summary>
    /// #undef identifier
    /// </summary>
    /// <param name="identifier"></param>
    [Command]
    public void Undef(string identifier)
    {
        var defines = engine.GetOrNew("Defines", () => new HashSet<string>());
        defines.Remove(identifier);
        if (logger is not null)
            LogUndefine(logger, identifier, default!);
    }
}