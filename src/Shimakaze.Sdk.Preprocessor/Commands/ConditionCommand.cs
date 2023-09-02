using Microsoft.Extensions.Logging;

using Shimakaze.Sdk.Preprocessor.Kernel;

namespace Shimakaze.Sdk.Preprocessor.Commands;

/// <summary>
/// Condition Commands if elif else endif
/// </summary>
public sealed class ConditionalCommand : ICommandSet
{
    private readonly Engine _engine;
    private readonly Logger<ConditionalCommand>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="logger"></param>
    public ConditionalCommand(Engine engine, Logger<ConditionalCommand>? logger = null)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// #if condition
    /// </summary>
    /// <param name="condition"></param>
    [Command]
    public void If([Param(@".*")]string condition)
    {
        var conditionStack = _engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());
        var conditionParser = _engine.GetOrNew("ConditionParser", () => new ConditionParser(_engine));

        _engine.CanWritable = conditionParser.Parse(condition);
        _logger?.LogDebug("If Condition: \"{condition}\" and computed value is: \"{value}\" ", condition, _engine.CanWritable);

        conditionStack.Push(new ConditionStatus(_engine.CanWritable, condition, "if"));
        _logger?.LogDebug("Push Stack: If");
    }

    /// <summary>
    /// #elif condition
    /// </summary>
    /// <param name="condition"></param>
    [Command]
    public void Elif([Param(@".*")]string condition)
    {
        var conditionStack = _engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());
        var conditionParser = _engine.GetOrNew("ConditionParser", () => new ConditionParser(_engine));

        var lastStatus = conditionStack.Pop();
        _logger?.LogDebug("Pop Stack: {tag}", lastStatus.Tag);

        _engine.CanWritable = false;
        if (lastStatus.IsMatched)
        {
            _logger?.LogDebug("ElIf is NOT Actived.");
        }
        else
        {
            _engine.CanWritable = conditionParser.Parse(condition);
            _logger?.LogDebug("ElIf Condition: \"{condition}\" and computed value is: \"{value}\" ", condition, _engine.CanWritable);
        }

        conditionStack.Push(new(_engine.CanWritable, condition, "elif"));
        _logger?.LogDebug("Push Stack: Elif");
    }

    /// <summary>
    /// #else
    /// </summary>
    [Command]
    public void Else()
    {
        var conditionStack = _engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());

        var lastStatus = conditionStack.Pop();
        _logger?.LogDebug("Pop Stack: {tag}", lastStatus.Tag);

        _engine.CanWritable = false;

        if (lastStatus.IsMatched)
        {
            _logger?.LogDebug("Else is NOT Actived.");
        }
        else
        {
            _engine.CanWritable = true;
            _logger?.LogDebug("Else computed value is: \"{value}\" ", _engine.CanWritable);
        }

        conditionStack.Push(new(_engine.CanWritable, string.Empty, "else"));
        _logger?.LogDebug("Push Stack: Else");
    }

    /// <summary>
    /// #endif
    /// </summary>
    [Command]
    public void Endif()
    {
        var conditionStack = _engine.GetOrNew("ConditionStack", () => new Stack<ConditionStatus>());

        var lastStatus = conditionStack.Pop();
        _logger?.LogDebug("Pop Stack: {tag}", lastStatus.Tag);

        _engine.CanWritable = true;
        _logger?.LogDebug("EndIf Actived!");
    }
}

file sealed class ConditionParser
{
    private readonly Engine _engine;

    public ConditionParser(Engine engine) => _engine = engine;

    public bool Parse(string condition)
    {
        condition = condition.Trim();
        switch (condition.ToLowerInvariant())
        {
            case "true": return true;
            case "false": return false;
            default: break;
        }

        var defines = _engine.GetOrNew("Defines", () => new HashSet<string>());

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
