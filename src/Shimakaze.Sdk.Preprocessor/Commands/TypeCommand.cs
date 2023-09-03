using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// Region Commands: region endregion
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TypeCommand : ICommandSet
{
    private readonly Engine _engine;
    private readonly Logger<TypeCommand>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="logger"></param>
    public TypeCommand(Engine engine, Logger<TypeCommand>? logger = null)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// #type
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="key">键</param>
    /// <param name="unit">单位名</param>
    [Command]
    public void Type(string type, string key, string unit)
    {
        _logger?.LogDebug("Adding {unit} as {key} in {type}", unit, key, type);
        string reg = $"""
        [{type}]
        {key}={unit}
        """;
        _engine.Writer.WriteLine(reg);
    }

    /// <inheritdoc cref="Type(string, string, string)"/>
    [Command]
    public void Type(string type, string key)
    {
        if (string.IsNullOrEmpty(_engine.FilePath))
        {
            _logger?.LogError("Cannot found current file's name");
            throw new NotSupportedException("Cannot found current file's name");
        }

        string unit = Path.GetFileName(_engine.FilePath).Split('.').First();
        Type(type, key, unit);
    }

    /// <inheritdoc cref="Type(string, string, string)"/>
    [Command]
    public void Type(string type)
    {
        var typeCounter = _engine.GetOrNew("TypeCounter", () => new TypeCounter());
        var key = typeCounter.Counter.ToString();
        Type(type, key);
    }

}

file sealed class TypeCounter
{
    private int _counter = 0;
    public int Counter => _counter++;
}
