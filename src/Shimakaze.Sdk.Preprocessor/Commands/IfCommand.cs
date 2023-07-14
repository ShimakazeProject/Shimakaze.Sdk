using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #if _________
/// </summary>
[PreprocessorCommand("if")]
public sealed class IfCommand : PreprocessorCommand
{
    private readonly IConditionParser _conditionParser;

    /// <inheritdoc />
    public IfCommand(IPreprocessorVariables preprocessor, IConditionParser conditionParser) : base(preprocessor)
    {
        _conditionParser = conditionParser;
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length is not 1)
            throw new ArgumentException("Invalid arguments");

        var conditionStack = _variable.ConditionStack;

        string condition = args[0];
        bool value = _conditionParser.Parse(condition);

        conditionStack.Push(new ConditionStatus(value, condition, "if"));
        _variable.WriteOutput = value;

        return Task.CompletedTask;
    }
}