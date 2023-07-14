using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #endif
/// </summary>
[PreprocessorCommand("endif")]
public sealed class EndIfCommand : PreprocessorCommand
{
    /// <inheritdoc />
    public EndIfCommand(IPreprocessorVariables preprocessor) : base(preprocessor)
    {
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var conditionStack = _variable.ConditionStack;
        _ = conditionStack.Pop();

        _variable.WriteOutput = true;

        return Task.CompletedTask;
    }
}