namespace Shimakaze.Sdk.Preprocessor;

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
    Stack<IConditionStatus> ConditionStack { get; }

    /// <summary>
    /// WriteOutput
    /// </summary>
    bool WriteOutput { get; set; }

    ///// <summary>
    ///// Creater
    ///// </summary>
    ///// <returns>IPreprocessorVariables</returns>
    //public static IPreprocessorVariables Create() => new PreprocessorVariables();
}
