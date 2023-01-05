namespace Shimakaze.Sdk.CompilerCollection.Preprocessor.Kernel;

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

internal record ConditionStatus
{
    public ConditionStatus(bool isMatched, string condition, string tag)
    {
        IsMatched = isMatched;
        Condition = condition;
        Tag = tag;
    }

    public bool IsMatched { get; set; }
    public string Tag { get; set; }
    public string Condition { get; set; }
}