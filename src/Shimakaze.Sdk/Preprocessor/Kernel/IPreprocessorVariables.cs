namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// IPreprocessorVariables
/// </summary>
public interface IPreprocessorVariables
{
    /// <summary>
    /// Defines
    /// </summary>
    HashSet<string> Defines { get; }

    /// <summary>
    /// ConditionStack
    /// </summary>
    internal Stack<ConditionStatus> ConditionStack { get; }

    /// <summary>
    /// WriteOutput
    /// </summary>
    internal bool WriteOutput { get; set; }

    /// <summary>
    /// Creater
    /// </summary>
    /// <returns>IPreprocessorVariables</returns>
    public static IPreprocessorVariables Create() => new PreprocessorVariables();
}
