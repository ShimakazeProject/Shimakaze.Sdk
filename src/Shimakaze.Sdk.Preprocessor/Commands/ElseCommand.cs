using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #else
/// </summary>
[PreprocessorCommand("else")]
public sealed class ElseCommand : PreprocessorCommand
{
    private readonly ILogger<ElseCommand>? _logger;
    private readonly IConditionParser _conditionParser;

    /// <inheritdoc/>
    public ElseCommand(IPreprocessorVariables preprocessor, IConditionParser conditionParser, ILogger<ElseCommand>? logger = null) : base(preprocessor)
    {
        _conditionParser = conditionParser;
        _logger = logger;
    }

    /// <inheritdoc/>
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var conditionStack = variable.ConditionStack;

        var lastStatus = conditionStack.Peek();

        lastStatus.Tag = "else";
        variable.WriteOutput = false;

        if (!lastStatus.IsMatched)
        {
            lastStatus.IsMatched = variable.WriteOutput = true;
        }

        return Task.CompletedTask;
    }
}