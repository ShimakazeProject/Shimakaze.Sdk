using Shimakaze.Sdk.Preprocessor;

namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// Preprocessor
/// </summary>
internal sealed class PreprocessorVariables : IPreprocessorVariables
{
    public HashSet<string> Defines { get; init; } = new();

    public bool WriteOutput { get; set; } = true;

    public Stack<IConditionStatus> ConditionStack { get; init; } = new();
}