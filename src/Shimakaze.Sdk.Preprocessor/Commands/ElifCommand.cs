using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #elif _________
/// </summary>
[PreprocessorCommand("elif")]
public sealed class ElifCommand : PreprocessorCommand
{
    private readonly IConditionParser _conditionParser;

    /// <inheritdoc />
    public ElifCommand(IPreprocessorVariables preprocessor, IConditionParser conditionParser) : base(preprocessor)
    {
        _conditionParser = conditionParser;
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length is not 1)
            throw new ArgumentException("Invalid arguments");

        var conditionStack = _variable.ConditionStack;

        var lastStatus = conditionStack.Peek();

        _variable.WriteOutput = false;

        if (!lastStatus.IsMatched)
        {
            string condition = args[0];
            bool value = _conditionParser.Parse(condition);
            lastStatus.Condition = condition;
            lastStatus.Tag = "elif";

            lastStatus.IsMatched = _variable.WriteOutput = value;
        }

        return Task.CompletedTask;
    }
}