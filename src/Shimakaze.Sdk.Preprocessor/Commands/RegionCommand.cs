using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// Region Commands: region endregion
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class RegionCommand : ICommandSet
{
    private readonly Logger<RegionCommand>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public RegionCommand(Logger<RegionCommand>? logger = null) => _logger = logger;

    /// <summary>
    /// #region
    /// </summary>
    [Command]
    public void Region() => _logger?.LogDebug("Region");

    /// <summary>
    /// #endregion
    /// </summary>
    [Command]
    public void Endregion() => _logger?.LogDebug("Endregion");
}