using Shimakaze.Sdk.Preprocessor;

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
        }

        if (_variables.Defines.Any(i => i.Equals(condition, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (condition.Contains("||"))
            return OR(condition);
        else if (condition.Contains("&&"))
            return AND(condition);
        else if (condition.TrimStart().StartsWith('!'))
            return NOT(condition);
        else
            return false;
    }

    private bool OR(string condition) => condition.Trim().Split("||").Any(Parse);

    private bool AND(string condition) => condition.Trim().Split("&&").All(Parse);

    private bool NOT(string condition)
    {
        condition = condition.Trim();
        return condition.StartsWith('!')
            ? !Parse(condition[1..])
            : Parse(condition);
    }
}