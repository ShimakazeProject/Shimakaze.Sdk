namespace Shimakaze.Sdk.Ra2lyn.Ini.Directives;

/// <summary>
/// 条件表达式求值器：优先级为 <c>!</c> → <c>&amp;&amp;</c> → <c>||</c>，括号最高。
/// </summary>
/// <remarks>
/// 标识符按 <see cref="DefineSet"/> 判定真假：已定义的行为真，未定义的为假。
/// 表达式会先完整解析再求值，因此语法错误总能被报出，而不是被短路掩盖。
/// </remarks>
public sealed class ConditionEvaluator(DefineSet defines)
{
    private readonly DefineSet _defines = defines;
    private IReadOnlyList<ConditionToken> _tokens = [];
    private int _position;

    /// <summary>
    /// 解析并求值表达式。
    /// </summary>
    /// <param name="text">表达式文本。</param>
    /// <returns>求值结果；失败时包含出错字符在表达式内的偏移与错误描述。</returns>
    public ConditionResult Evaluate(string text)
    {
        if (ConditionLexer.TryTokenize(text, out var tokens, out int lexOffset, out string lexError) is false)
            return ConditionResult.Failure(lexOffset, lexError);

        _tokens = tokens;
        _position = 0;
        ParseResult result = ParseOr();
        return result.Kind is ParseResultKind.Success
            ? ConditionResult.Success(result.Value)
            : ConditionResult.Failure(result.Offset, result.Message);
    }

    private ParseResult ParseOr()
    {
        ParseResult left = ParseAnd();
        if (left.Kind is ParseResultKind.Failure)
            return left;

        bool value = left.Value;
        while (Peek()?.Kind is ConditionTokenKind.Or)
        {
            Advance();
            ParseResult right = ParseAnd();
            if (right.Kind is ParseResultKind.Failure)
                return right;

            value |= right.Value;
        }

        return ParseResult.SuccessResult(value);
    }

    private ParseResult ParseAnd()
    {
        ParseResult left = ParseUnary();
        if (left.Kind is ParseResultKind.Failure)
            return left;

        bool value = left.Value;
        while (Peek()?.Kind is ConditionTokenKind.And)
        {
            Advance();
            ParseResult right = ParseUnary();
            if (right.Kind is ParseResultKind.Failure)
                return right;

            value &= right.Value;
        }

        return ParseResult.SuccessResult(value);
    }

    private ParseResult ParseUnary()
    {
        ConditionToken? token = Peek();
        if (token is null)
            return ParseResult.FailureResult(PeekOffset(), "条件表达式意外结束");

        switch (token.Kind)
        {
            case ConditionTokenKind.Not:
                Advance();
                ParseResult inner = ParseUnary();
                if (inner.Kind is ParseResultKind.Failure)
                    return inner;

                return ParseResult.SuccessResult(inner.Value is false);

            case ConditionTokenKind.LeftParen:
                Advance();
                ParseResult grouped = ParseOr();
                if (grouped.Kind is ParseResultKind.Failure)
                    return grouped;

                if (Peek()?.Kind is not ConditionTokenKind.RightParen)
                    return ParseResult.FailureResult(PeekOffset(), "缺少右括号 ')'");

                Advance();
                return ParseResult.SuccessResult(grouped.Value);

            case ConditionTokenKind.Identifier:
                Advance();
                return ParseResult.SuccessResult(_defines.Contains(token.Text));

            default:
                return ParseResult.FailureResult(PeekOffset(), $"意外的记号 '{token.Text}'");
        }
    }

    private ConditionToken? Peek()
        => _position < _tokens.Count ? _tokens[_position] : null;

    private void Advance()
        => _position++;

    private int PeekOffset()
        => Peek()?.Start ?? (_tokens.Count is 0 ? 0 : _tokens[^1].Start + _tokens[^1].Text.Length);
}

/// <summary>
/// 条件求值结果。
/// </summary>
/// <param name="Succeeded">是否求值成功。</param>
/// <param name="Value">成功时的布尔结果。</param>
/// <param name="ErrorOffset">失败时出错字符在表达式内的偏移；成功时为 -1。</param>
/// <param name="ErrorMessage">失败时的错误描述；成功时为空串。</param>
public sealed record class ConditionResult(bool Succeeded, bool Value, int ErrorOffset, string ErrorMessage)
{
    /// <summary>
    /// 构造成功结果。
    /// </summary>
    public static ConditionResult Success(bool value)
        => new(true, value, -1, string.Empty);

    /// <summary>
    /// 构造失败结果。
    /// </summary>
    public static ConditionResult Failure(int errorOffset, string errorMessage)
        => new(false, false, errorOffset, errorMessage);
}

internal enum ParseResultKind
{
    Success,
    Failure,
}

internal readonly record struct ParseResult(ParseResultKind Kind, bool Value, int Offset, string Message)
{
    public static ParseResult SuccessResult(bool value)
        => new(ParseResultKind.Success, value, -1, string.Empty);

    public static ParseResult FailureResult(int offset, string message)
        => new(ParseResultKind.Failure, false, offset, message);
}

/// <summary>
/// 已定义的编译宏集合；名称大小写敏感，与 C# 保持一致。
/// </summary>
public sealed class DefineSet
{
    private readonly HashSet<string> _names;

    /// <summary>
    /// 初始化宏集合。
    /// </summary>
    /// <param name="names">已定义的宏名。</param>
    public DefineSet(IEnumerable<string> names)
        => _names = [.. names];

    /// <summary>
    /// 空集合：所有标识符都为假。
    /// </summary>
    public static DefineSet Empty { get; } = new([]);

    /// <summary>
    /// 已定义宏的数量。
    /// </summary>
    public int Count
        => _names.Count;

    /// <summary>
    /// 判断宏是否已定义。
    /// </summary>
    /// <param name="name">宏名。</param>
    public bool Contains(string name)
        => _names.Contains(name);
}
