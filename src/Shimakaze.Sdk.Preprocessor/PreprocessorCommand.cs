using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor;

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
