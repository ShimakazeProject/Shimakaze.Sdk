using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #undef _________
/// </summary>
[PreprocessorCommand("undef")]
public sealed class UndefCommand : PreprocessorCommand
{
    /// <inheritdoc />
    public UndefCommand(IPreprocessorVariables variable) : base(variable)
    {
    }

    /// <inheritdoc />

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length is not 1)
            throw new ArgumentException("Invalid arguments");

        string identifier = args[0];

        _variable.Defines.Remove(identifier);

        return Task.CompletedTask;
    }
}