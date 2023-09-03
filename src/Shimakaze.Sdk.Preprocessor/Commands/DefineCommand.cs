using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;
/// <summary>
/// Define Commands: define undef
/// </summary>
public sealed class DefineCommand : ICommandSet
{
    private readonly Engine _engine;
    private readonly Logger<DefineCommand>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="logger"></param>
    public DefineCommand(Engine engine, Logger<DefineCommand>? logger = null)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// #define identifier
    /// </summary>
    /// <param name="identifier"></param>
    [Command]
    public void Define(string identifier)
    {
        var defines = _engine.GetOrNew("Defines", () => new HashSet<string>());
        defines.Add(identifier);
        _logger?.LogDebug("Define {identifier}", identifier);
    }

    /// <summary>
    /// #undef identifier
    /// </summary>
    /// <param name="identifier"></param>
    [Command]
    public void Undef(string identifier)
    {
        var defines = _engine.GetOrNew("Defines", () => new HashSet<string>());
        defines.Remove(identifier);
        _logger?.LogDebug("Undefine {identifier}", identifier);
    }
}