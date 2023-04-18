using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #elif _________
/// </summary>
[PreprocessorCommand("elif")]
public sealed class ElifCommand : PreprocessorCommand
{
    private readonly ILogger<ElifCommand>? _logger;
    private readonly IConditionParser _conditionParser;

    /// <inheritdoc/>
    public ElifCommand(IPreprocessorVariables preprocessor, IConditionParser conditionParser, ILogger<ElifCommand>? logger = null) : base(preprocessor)
    {
        _conditionParser = conditionParser;
        _logger = logger;
    }

    /// <inheritdoc/>
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length is not 1)
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
