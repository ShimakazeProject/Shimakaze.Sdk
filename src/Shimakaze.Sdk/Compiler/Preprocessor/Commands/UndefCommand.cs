using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Compiler.Preprocessor.Commands;

/// <summary>
/// #undef _________
/// </summary>
[PreprocessorCommand("undef")]
public sealed class UndefCommand : PreprocessorCommand
{
    private readonly ILogger<UndefCommand> _logger;
    /// <inheritdoc/>
    public UndefCommand(IPreprocessorVariables variable, ILogger<UndefCommand> logger) : base(variable)
    {
        _logger = logger;
    }
    /// <inheritdoc/>

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1)
            throw new ArgumentException("Invalid arguments");

        string identifier = args[0];

        variable.Defines.Remove(identifier);

        return Task.CompletedTask;
    }
}
