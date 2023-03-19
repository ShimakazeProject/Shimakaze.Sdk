namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// IConditionParser
/// </summary>
/// <remarks>
/// <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/preprocessor-directives#conditional-compilation"/>
/// </remarks>
public interface IConditionParser
{
    /// <summary>
    /// Parse
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    bool Parse(string condition);

    /// <summary>
    /// Create an Instance
    /// </summary>
    /// <param name="defines"></param>
    /// <returns></returns>
    static IConditionParser Create(params string[] defines) => new ConditionParser(new PreprocessorVariables()
    {
        Defines = new(defines)
    });
}
