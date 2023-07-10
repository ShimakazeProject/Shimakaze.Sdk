using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #endregion
/// </summary>
[PreprocessorCommand("endregion")]
public sealed class EndregionCommand : PreprocessorCommand
{
    /// <inheritdoc />
    public EndregionCommand(IPreprocessorVariables variable) : base(variable)
    {
    }

    /// <inheritdoc />

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}