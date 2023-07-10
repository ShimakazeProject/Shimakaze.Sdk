using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #else
/// </summary>
[PreprocessorCommand("else")]
public sealed class ElseCommand : PreprocessorCommand
{
    /// <inheritdoc />
    public ElseCommand(IPreprocessorVariables preprocessor) : base(preprocessor)
    {
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var conditionStack = _variable.ConditionStack;

        var lastStatus = conditionStack.Peek();

        lastStatus.Tag = "else";
        _variable.WriteOutput = false;

        if (!lastStatus.IsMatched)
        {
            lastStatus.IsMatched = _variable.WriteOutput = true;
        }

        return Task.CompletedTask;
    }
}