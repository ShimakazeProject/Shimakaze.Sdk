using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.CompilerCollection.Preprocessor.Kernel;

namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Commands;

/// <summary>
/// #elif _________
/// </summary>
[PreprocessorCommand("elif")]
public sealed class ElifCommand : PreprocessorCommand
{
    private readonly ILogger<ElifCommand> _logger;
    private readonly IConditionParser _conditionParser;
    /// <inheritdoc/>
    public ElifCommand(IPreprocessorVariables preprocessor, ILogger<ElifCommand> logger, IConditionParser conditionParser) : base(preprocessor)
    {
        _logger = logger;
        _conditionParser = conditionParser;
    }

    /// <inheritdoc/>
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1)
            throw new ArgumentException("Invalid arguments");

        var conditionStack = variable.ConditionStack;

        var lastStatus = conditionStack.Peek();

        variable.WriteOutput = false;

        if (!lastStatus.IsMatched)
        {
            string condition = args[0];
            bool value = _conditionParser.Parse(condition);
            lastStatus.Condition = condition;
            lastStatus.Tag = "elif";

            lastStatus.IsMatched = variable.WriteOutput = value;
        }

        return Task.CompletedTask;
    }
}
