using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Compiler.Preprocessor.Commands;

/// <summary>
/// #define _________
/// </summary>
[PreprocessorCommand("define")]
public sealed class DefineCommand : PreprocessorCommand
{
    private readonly ILogger<DefineCommand> _logger;
    /// <inheritdoc/>
    public DefineCommand(IPreprocessorVariables variable, ILogger<DefineCommand> logger) : base(variable)
    {
        _logger = logger;
    }
    /// <inheritdoc/>

    public override Task ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1)
            throw new ArgumentException("Invalid arguments");

        string identifier = args[0];

        variable.Defines.Add(identifier);

        return Task.CompletedTask;
    }
}
