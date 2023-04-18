using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #endregion
/// </summary>
[PreprocessorCommand("endregion")]
public sealed class EndregionCommand : PreprocessorCommand
{
    private readonly ILogger<EndregionCommand>? _logger;

    /// <inheritdoc/>
    public EndregionCommand(IPreprocessorVariables variable, ILogger<EndregionCommand>? logger = null) : base(variable)
    {
        _logger = logger;
    }

    /// <inheritdoc/>

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
