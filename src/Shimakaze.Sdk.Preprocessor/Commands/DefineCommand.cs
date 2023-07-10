using Microsoft.Extensions.Logging;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// #define _________
/// </summary>
[PreprocessorCommand("define")]
public sealed class DefineCommand : PreprocessorCommand
{
    /// <inheritdoc />
    public DefineCommand(IPreprocessorVariables variable) : base(variable)
    {
    }

    /// <inheritdoc />

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length is not 1)
            throw new ArgumentException("Invalid arguments");

        string identifier = args[0];

        _variable.Defines.Add(identifier);

        return Task.CompletedTask;
    }
}