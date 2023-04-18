using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #undef _________
/// </summary>
[PreprocessorCommand("undef")]
public sealed class UndefCommand : PreprocessorCommand
{
    private readonly ILogger<UndefCommand>? _logger;

    /// <inheritdoc/>
    public UndefCommand(IPreprocessorVariables variable, ILogger<UndefCommand>? logger = null) : base(variable)
    {
        _logger = logger;
    }

    /// <inheritdoc/>

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length is not 1)
            throw new ArgumentException("Invalid arguments");

        string identifier = args[0];

        variable.Defines.Remove(identifier);

        return Task.CompletedTask;
    }
}
