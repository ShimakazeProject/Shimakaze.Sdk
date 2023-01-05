namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Kernel;

/// <summary>
/// IPreprocessorVariablesBuilder
/// </summary>
public interface IPreprocessorVariablesBuilder
{
    /// <summary>
    /// AddDefine
    /// </summary>
    /// <param name="define"></param>
    /// <returns></returns>
    IPreprocessorVariablesBuilder AddDefine(string define);
    /// <summary>
    /// AddDefines
    /// </summary>
    /// <param name="defines"></param>
    /// <returns></returns>
    IPreprocessorVariablesBuilder AddDefines(params string[] defines);
    /// <summary>
    /// AddDefines
    /// </summary>
    /// <param name="defines"></param>
    /// <returns></returns>
    IPreprocessorVariablesBuilder AddDefines(IEnumerable<string> defines);
    /// <summary>
    /// Build
    /// </summary>
    /// <returns></returns>
    IPreprocessorVariables Build();
}
