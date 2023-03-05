namespace Shimakaze.Sdk.Compiler.Preprocessor.Kernel;

/// <summary>
/// IPreprocessor
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
}
