namespace Shimakaze.Sdk.Ra2lyn.Ini.Directives;

/// <summary>
/// 条件表达式词法记号。
/// </summary>
/// <param name="Kind">记号种类。</param>
/// <param name="Text">原始文本。</param>
/// <param name="Start">在表达式内的起始偏移。</param>
public sealed record class ConditionToken(ConditionTokenKind Kind, string Text, int Start);

/// <summary>
/// 条件表达式记号种类。
/// </summary>
public enum ConditionTokenKind
{
    /// <summary>
    /// 标识符，作为宏名；未定义时为假。
    /// </summary>
    Identifier,

    /// <summary>
    /// 逻辑非 <c>!</c>。
    /// </summary>
    Not,

    /// <summary>
    /// 逻辑与 <c>&amp;&amp;</c>。
    /// </summary>
    And,

    /// <summary>
    /// 逻辑或 <c>||</c>。
    /// </summary>
    Or,

    /// <summary>
    /// 左括号 <c>(</c>。
    /// </summary>
    LeftParen,

    /// <summary>
    /// 右括号 <c>)</c>。
    /// </summary>
    RightParen,
}

/// <summary>
 /// 条件表达式词法分析器。
 /// </summary>
 /// <remarks>
 /// 只接受布尔运算：标识符、<c>!</c>、<c>&amp;&amp;</c>、<c>||</c> 与圆括号。
 /// 不支持比较运算、算术运算与字面量；出现其它字符时返回失败与错误位置。
 /// </remarks>
public static class ConditionLexer
{
    /// <summary>
    /// 把表达式文本拆分为记号序列。
    /// </summary>
    /// <param name="text">表达式文本。</param>
    /// <param name="tokens">成功时的记号序列。</param>
    /// <param name="errorOffset">失败时出错字符在表达式内的偏移。</param>
    /// <param name="errorMessage">失败时的错误描述。</param>
    public static bool TryTokenize(string text, out IReadOnlyList<ConditionToken> tokens, out int errorOffset, out string errorMessage)
    {
        List<ConditionToken> result = [];
        int position = 0;

        while (position < text.Length)
        {
            char current = text[position];
            if (current is ' ' or '\t')
            {
                position++;
                continue;
            }

            int start = position;
            switch (current)
            {
                case '!':
                    result.Add(new(ConditionTokenKind.Not, "!", start));
                    position++;
                    continue;
                case '&' when position + 1 < text.Length && text[position + 1] is '&':
                    result.Add(new(ConditionTokenKind.And, "&&", start));
                    position += 2;
                    continue;
                case '|' when position + 1 < text.Length && text[position + 1] is '|':
                    result.Add(new(ConditionTokenKind.Or, "||", start));
                    position += 2;
                    continue;
                case '(':
                    result.Add(new(ConditionTokenKind.LeftParen, "(", start));
                    position++;
                    continue;
                case ')':
                    result.Add(new(ConditionTokenKind.RightParen, ")", start));
                    position++;
                    continue;
            }

            if (IsIdentifierStart(current))
            {
                while (position < text.Length && IsIdentifierPart(text[position]))
                    position++;

                result.Add(new(ConditionTokenKind.Identifier, text[start..position], start));
                continue;
            }

            if (current is '&' or '|')
            {
                tokens = [];
                errorOffset = start;
                errorMessage = $"'{current}' 需要成对出现";
                return false;
            }

            tokens = [];
            errorOffset = start;
            errorMessage = $"条件表达式不支持字符 '{current}'，只允许标识符、!、&&、|| 与括号";
            return false;
        }

        tokens = result;
        errorOffset = -1;
        errorMessage = string.Empty;
        return true;
    }

    private static bool IsIdentifierStart(char value)
        => value is '_' || char.IsLetter(value);

    private static bool IsIdentifierPart(char value)
        => value is '_' || char.IsLetterOrDigit(value);
}
