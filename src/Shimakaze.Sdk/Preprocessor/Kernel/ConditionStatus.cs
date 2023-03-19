namespace Shimakaze.Sdk.Preprocessor.Kernel;

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