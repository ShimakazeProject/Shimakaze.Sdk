using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Compiler.Preprocessor.Commands;
/// <summary>
/// #endif
/// </summary>
[PreprocessorCommand("endif")]
public sealed class EndIfCommand : PreprocessorCommand
{
    private readonly ILogger<EndIfCommand> _logger;
    private readonly IConditionParser _conditionParser;
    /// <inheritdoc/>
    public EndIfCommand(IPreprocessorVariables preprocessor, ILogger<EndIfCommand> logger, IConditionParser conditionParser) : base(preprocessor)
    {
        _logger = logger;
        _conditionParser = conditionParser;
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
