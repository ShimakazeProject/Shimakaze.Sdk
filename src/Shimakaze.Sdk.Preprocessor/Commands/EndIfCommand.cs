using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #endif
/// </summary>
[PreprocessorCommand("endif")]
public sealed class EndIfCommand : PreprocessorCommand
{
    private readonly ILogger<EndIfCommand>? _logger;
    private readonly IConditionParser _conditionParser;

    /// <inheritdoc/>
    public EndIfCommand(IPreprocessorVariables preprocessor, IConditionParser conditionParser, ILogger<EndIfCommand>? logger = null) : base(preprocessor)
    {
        _conditionParser = conditionParser;
        _logger = logger;
    }

    /// <inheritdoc/>
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        var conditionStack = variable.ConditionStack;

        var lastStatus = conditionStack.Pop();

        variable.WriteOutput = true;

        return Task.CompletedTask;
    }
}