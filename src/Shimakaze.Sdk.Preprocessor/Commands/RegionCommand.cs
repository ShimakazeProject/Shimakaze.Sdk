using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// Region Commands: region endregion
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="logger"></param>

public sealed class RegionCommand(Logger<RegionCommand>? logger = null)
{
    private static readonly Action<ILogger, Exception> LogRegion = LoggerMessage.Define(LogLevel.Debug, 0, "Region");
    private static readonly Action<ILogger, Exception> LogEndregion = LoggerMessage.Define(LogLevel.Debug, 0, "Endregion");

    /// <summary>
    /// #region
    /// </summary>
    [Command]
    public void Region()
    {
        if (logger is not null)
            LogRegion(logger, default!);
    }

    /// <summary>
    /// #endregion
    /// </summary>
    [Command]
    public void Endregion()
    {
        if (logger is not null)
            LogEndregion(logger, default!);
    }
}