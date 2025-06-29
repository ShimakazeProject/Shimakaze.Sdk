namespace Shimakaze.Sdk.Inilyn.Command;

internal static class ConditionalCommand
{
    [Command("if")]
    public static void If(ParserContext context, string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return;

        Stack<ConditionStatus> conditionStack = context.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());
        ConditionParser conditionParser = context.GetOrNew("ConditionParser", () => new ConditionParser(context));

        context.CanWritable = conditionParser.Parse(condition);

        conditionStack.Push(new ConditionStatus(context.CanWritable, condition, "if"));
    }

    [Command("elif")]
    public static void Elif(ParserContext context, string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return;

        Stack<ConditionStatus> conditionStack = context.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());
        ConditionParser conditionParser = context.GetOrNew("ConditionParser", () => new ConditionParser(context));

        ConditionStatus lastStatus = conditionStack.Pop();

        context.CanWritable = false;
        if (!lastStatus.IsMatched)
            context.CanWritable = conditionParser.Parse(condition);


        conditionStack.Push(new(context.CanWritable, condition, "elif"));
    }

    [Command("else")]
    public static void Else(ParserContext context)
    {
        Stack<ConditionStatus> conditionStack = context.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());

        ConditionStatus lastStatus = conditionStack.Pop();

        context.CanWritable = false;

        if (!lastStatus.IsMatched)
            context.CanWritable = true;

        conditionStack.Push(new(context.CanWritable, string.Empty, "else"));
    }

    [Command("endif")]
    public static void Endif(ParserContext context)
    {
        Stack<ConditionStatus> conditionStack = context.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());

        ConditionStatus lastStatus = conditionStack.Pop();

        context.CanWritable = true;
    }
}

file sealed class ConditionParser(ParserContext context)
{
    public bool Parse(string condition)
    {
        condition = condition.Trim();
        switch (condition.ToLowerInvariant())
        {
            case "true": return true;
            case "false": return false;
            default: break;
        }

        HashSet<string> defines = context.GetOrNew("Defines", () => new HashSet<string>());

        return defines.Any(i => i.Equals(condition, StringComparison.OrdinalIgnoreCase))
            || (condition.Contains("||")
            ? OR(condition)
            : condition.Contains("&&")
            ? AND(condition)
            : condition.TrimStart().StartsWith('!') && NOT(condition));
    }

    private bool AND(string condition)
    {
        return condition.Trim().Split("&&").All(Parse);
    }

    private bool NOT(string condition)
    {
        condition = condition.Trim();
        return condition.StartsWith('!')
            ? !Parse(condition[1..])
            : Parse(condition);
    }

    private bool OR(string condition)
    {
        return condition.Trim().Split("||").Any(Parse);
    }
}

file sealed record ConditionStatus(bool IsMatched, string Condition, string Tag);
