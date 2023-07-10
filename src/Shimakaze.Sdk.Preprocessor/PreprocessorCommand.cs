namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// PreprocessorCommand Base
/// </summary>
public abstract class PreprocessorCommand : IPreprocessorCommand
{
    /// <summary>
    /// Preprocessor Instance
    /// </summary>
    protected readonly IPreprocessorVariables _variable;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="variable"> Preprocessor Instance </param>
    public PreprocessorCommand(IPreprocessorVariables variable) => this._variable = variable;

    /// <inheritdoc />
    public abstract Task ExecuteAsync(string[] args, CancellationToken cancellationToken);
}