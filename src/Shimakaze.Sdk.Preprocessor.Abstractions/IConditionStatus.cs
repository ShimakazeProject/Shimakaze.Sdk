namespace Shimakaze.Sdk.Preprocessor;

public interface IConditionStatus
{
    string Condition { get; set; }
    bool IsMatched { get; set; }
    string Tag { get; set; }
}