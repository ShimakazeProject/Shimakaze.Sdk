using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// Condition Commands if elif else endif
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="engine"></param>
/// <param name="logger"></param>
public sealed class ConditionalCommand(Engine engine, Logger<ConditionalCommand>? logger = null)
{
    private static readonly Action<ILogger, string, bool, Exception> Condition = LoggerMessage.Define<string, bool>(LogLevel.Error, 1, "Condition: \"{Condition}\" and computed value is: \"{Value}\".");
    private static readonly Action<ILogger, Exception> PushIf = LoggerMessage.Define(LogLevel.Error, 1, "Push Stack: If");
    private static readonly Action<ILogger, Exception> PushElif = LoggerMessage.Define(LogLevel.Error, 1, "Push Stack: Elif");
    private static readonly Action<ILogger, Exception> PushElse = LoggerMessage.Define(LogLevel.Error, 1, "Push Stack: Else");
    private static readonly Action<ILogger, string, Exception> Pop = LoggerMessage.Define<string>(LogLevel.Error, 1, "Push Stack: {Tag}");
    private static readonly Action<ILogger, Exception> ElifNotActived = LoggerMessage.Define(LogLevel.Error, 1, "ElIf is NOT Actived.");
    private static readonly Action<ILogger, Exception> ElseNotActived = LoggerMessage.Define(LogLevel.Error, 1, "Else is NOT Actived.");
    private static readonly Action<ILogger, Exception> EndIfActived = LoggerMessage.Define(LogLevel.Error, 1, "EndIf Actived!");


    /// <summary>
    /// #if condition
    /// </summary>
    /// <param name="condition"></param>
    [Command]
    public void If([Param(@".*")] string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentNullException(nameof(condition));

        var conditionStack = engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());
        var conditionParser = engine.GetOrNew("ConditionParser", () => new ConditionParser(engine));

        engine.CanWritable = conditionParser.Parse(condition);
        if (logger is not null)
            Condition(logger, condition, engine.CanWritable, default!);

        conditionStack.Push(new ConditionStatus(engine.CanWritable, condition, "if"));
        if (logger is not null)
            PushIf(logger, default!);
    }

    /// <summary>
    /// #elif condition
    /// </summary>
    /// <param name="condition"></param>
    [Command]
    public void Elif([Param(@".*")] string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentNullException(nameof(condition));

        var conditionStack = engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());
        var conditionParser = engine.GetOrNew("ConditionParser", () => new ConditionParser(engine));

        var lastStatus = conditionStack.Pop();
        if (logger is not null)
            Pop(logger, lastStatus.Tag, default!);

        engine.CanWritable = false;
        if (lastStatus.IsMatched)
        {
            if (logger is not null)
                ElifNotActived(logger, default!);
        }
        else
        {
            engine.CanWritable = conditionParser.Parse(condition);
            if (logger is not null)
                Condition(logger, condition, engine.CanWritable, default!);
        }

        conditionStack.Push(new(engine.CanWritable, condition, "elif"));
        if (logger is not null)
            PushElif(logger, default!);
    }

    /// <summary>
    /// #else
    /// </summary>
    [Command]
    public void Else()
    {
        var conditionStack = engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());

        var lastStatus = conditionStack.Pop();
        if (logger is not null)
            Pop(logger, lastStatus.Tag, default!);

        engine.CanWritable = false;

        if (lastStatus.IsMatched)
        {
            if (logger is not null)
                ElseNotActived(logger, default!);
        }
        else
        {
            engine.CanWritable = true;
            if (logger is not null)
                Condition(logger, "{ELSE}", engine.CanWritable, default!);
        }

        conditionStack.Push(new(engine.CanWritable, string.Empty, "else"));
        if (logger is not null)
            PushElse(logger, default!);
    }

    /// <summary>
    /// #endif
    /// </summary>
    [Command]
    public void Endif()
    {
        var conditionStack = engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());

        var lastStatus = conditionStack.Pop();
        if (logger is not null)
            Pop(logger, lastStatus.Tag, default!);

        engine.CanWritable = true;
        if (logger is not null)
            EndIfActived(logger, default!);
    }
}

file sealed class ConditionParser(Engine engine)
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

        var defines = engine.GetOrNew("Defines", () => new HashSet<string>());

        return defines.Any(i => i.Equals(condition, StringComparison.OrdinalIgnoreCase))
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

file sealed record ConditionStatus(bool IsMatched, string Condition, string Tag);