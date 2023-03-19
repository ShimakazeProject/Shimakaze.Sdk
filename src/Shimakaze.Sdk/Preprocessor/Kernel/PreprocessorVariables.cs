namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// Preprocessor
/// </summary>
internal sealed class PreprocessorVariables : IPreprocessorVariables
{
    public HashSet<string> Defines { get; init; } = new();
    public bool WriteOutput { get; set; } = true;

    public Stack<ConditionStatus> ConditionStack { get; init; } = new();
}
