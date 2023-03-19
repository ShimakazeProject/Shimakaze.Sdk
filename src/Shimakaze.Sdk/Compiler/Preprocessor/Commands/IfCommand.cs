using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Compiler.Preprocessor.Commands;

/// <summary>
/// #if _________
/// </summary>
[PreprocessorCommand("if")]
public sealed class IfCommand : PreprocessorCommand
{
    private readonly ILogger<IfCommand>? _logger;
    private readonly IConditionParser _conditionParser;
    /// <inheritdoc/>
    public IfCommand(IPreprocessorVariables preprocessor, IConditionParser conditionParser, ILogger<IfCommand>? logger = null) : base(preprocessor)
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

        string condition = args[0];
        bool value = _conditionParser.Parse(condition);

        conditionStack.Push(new(value, condition, "if"));
        variable.WriteOutput = value;

        return Task.CompletedTask;
    }
}
