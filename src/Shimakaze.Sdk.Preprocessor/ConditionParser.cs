namespace Shimakaze.Sdk.Preprocessor;

internal class ConditionParser : IConditionParser
{
    private readonly IPreprocessorVariables _variables;

    public ConditionParser(IPreprocessorVariables variables)
    {
        _variables = variables;
    }

    public bool Parse(string condition)
    {
        condition = condition.Trim();
        switch (condition.ToLowerInvariant())
        {
            case "true": return true;
            case "false": return false;
            default: break;
        }

        return _variables.Defines.Any(i => i.Equals(condition, StringComparison.OrdinalIgnoreCase))
            || (condition.Contains("||")
            ? OR(condition)
            : condition.Contains("&&")
            ? AND(condition)
            : condition.TrimStart().StartsWith('!') && NOT(condition));
    }

    private bool AND(string condition) => condition.Trim().Split("&&").All(Parse);

    private bool NOT(string condition)
    {
        condition = condition.Trim();
        return condition.StartsWith('!')
            ? !Parse(condition[1..])
            : Parse(condition);
    }

    private bool OR(string condition) => condition.Trim().Split("||").Any(Parse);
}