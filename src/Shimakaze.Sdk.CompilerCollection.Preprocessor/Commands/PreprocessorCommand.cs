using Shimakaze.Sdk.CompilerCollection.Preprocessor.Kernel;

namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Commands;

/// <summary>
/// PreprocessorCommand Base
/// </summary>
public abstract class PreprocessorCommand : IPreprocessorCommand
{
    /// <summary>
    /// Preprocessor Instance
    /// </summary>
    protected readonly IPreprocessorVariables variable;
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="variable">Preprocessor Instance</param>
    public PreprocessorCommand(IPreprocessorVariables variable) => this.variable = variable;

    /// <inheritdoc/>
    public abstract Task ExecuteAsync(string[] args, CancellationToken cancellationToken);
}
