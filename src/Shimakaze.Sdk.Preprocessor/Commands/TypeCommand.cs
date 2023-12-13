using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// Region Commands: region endregion
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="engine"></param>
/// <param name="logger"></param>
public sealed class TypeCommand(Engine engine, Logger<TypeCommand>? logger = null)
{
    private static readonly Action<ILogger, Exception> InvalidOperation = LoggerMessage.Define(LogLevel.Error, 1, "InvalidOperation.");
    private static readonly Action<ILogger, string, string, string, Exception> LogType = LoggerMessage.Define<string, string, string>(LogLevel.Debug, 0, "Adding {Unit} as {Key} in {Type}.");

    /// <summary>
    /// #type
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="key">键</param>
    /// <param name="unit">单位名</param>
    [Command]
    public void Type(string type, string key, string unit)
    {
        if (logger is not null)
            LogType(logger, unit, key, type, default!);
        string reg = $"""
        [{type}]
        {key}={unit}
        """;
        engine.Writer.WriteLine(reg);
    }

    /// <inheritdoc cref="Type(string, string, string)"/>
    [Command]
    public void Type(string type, string key)
    {
        if (string.IsNullOrEmpty(engine.FilePath))
        {
            InvalidOperationException ex = new("Cannot found current file's name");
            if (logger is not null)
                InvalidOperation(logger, ex);
            throw ex;
        }

        string unit = Path.GetFileName(engine.FilePath).Split('.').First();
        Type(type, key, unit);
    }

    /// <inheritdoc cref="Type(string, string, string)"/>
    [Command]
    public void Type(string type)
    {
        var typeCounter = engine.GetOrNew("TypeCounter", () => new TypeCounter());
        var key = typeCounter.Counter.ToString(CultureInfo.InvariantCulture);
        Type(type, key);
    }

}

file sealed class TypeCounter
{
    private int _counter;
    public int Counter => _counter++;
}