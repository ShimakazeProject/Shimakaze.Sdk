using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #region _________
/// </summary>
[PreprocessorCommand("region")]
public sealed class RegionCommand : PreprocessorCommand
{

    /// <inheritdoc />
    public RegionCommand(IPreprocessorVariables variable) : base(variable)
    {
    }

    /// <inheritdoc />

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}