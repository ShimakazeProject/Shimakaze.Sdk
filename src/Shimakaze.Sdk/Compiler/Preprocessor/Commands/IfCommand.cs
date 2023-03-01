using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Compiler.Preprocessor.Commands;

/// <summary>
/// #if _________
/// </summary>
[PreprocessorCommand("if")]
public sealed class IfCommand : PreprocessorCommand
{
    private readonly ILogger<IfCommand> _logger;
    private readonly IConditionParser _conditionParser;
    /// <inheritdoc/>
    public IfCommand(IPreprocessorVariables preprocessor, ILogger<IfCommand> logger, IConditionParser conditionParser) : base(preprocessor)
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

        string condition = args[0];
        bool value = _conditionParser.Parse(condition);

        _logger.LogTrace($"[ConditionStack]::Push(\"{condition}\", \"{value}\", \"if\")");

        conditionStack.Push(new(value, condition, "if"));
        variable.WriteOutput = value;

        return Task.CompletedTask;
    }
}
