using System.Text;

namespace Shimakaze.Sdk.Ra2lyn.Common.Template;

/// <summary>
/// <c>where</c> 表达式词法记号。
/// </summary>
/// <param name="Kind">记号种类。</param>
/// <param name="Text">原始文本；字符串字面量已去掉引号并处理转义，标识符保留原文。</param>
/// <param name="Start">在表达式内的起始偏移。</param>
/// <param name="End">在表达式内的结束偏移（不含）。</param>
/// <param name="IsConditionalAccess">
/// 仅对 <see cref="WhereTokenKind.Dot"/> 有意义：为 <see langword="true"/> 表示这是可选链 <c>?.</c>。
/// </param>
public sealed record class WhereToken(WhereTokenKind Kind, string Text, int Start, int End, bool IsConditionalAccess = false);

/// <summary>
/// <c>where</c> 表达式记号种类（TODO.md §4.3.1）。
/// </summary>
public enum WhereTokenKind
{
    /// <summary>标识符，如 <c>Techlevel</c>、<c>contains</c>、<c>where</c>。</summary>
    Identifier,

    /// <summary>节名引用 <c>$Section.Id</c>（TODO.md §4.3.3），文本保存以 <c>$</c> 起的原文。</summary>
    SectionId,

    /// <summary>数字字面量。</summary>
    Number,

    /// <summary>字符串字面量 <c>"..."</c>。</summary>
    String,

    /// <summary>布尔字面量 <c>true</c> / <c>false</c>（大小写不敏感）。</summary>
    Boolean,

    /// <summary><c>&amp;&amp;</c></summary>
    And,

    /// <summary><c>||</c></summary>
    Or,

    /// <summary><c>!</c></summary>
    Not,

    /// <summary><c>==</c></summary>
    Equal,

    /// <summary><c>!=</c></summary>
    NotEqual,

    /// <summary><c>&gt;=</c></summary>
    GreaterOrEqual,

    /// <summary><c>&lt;=</c></summary>
    LessOrEqual,

    /// <summary><c>&gt;</c></summary>
    GreaterThan,

    /// <summary><c>&lt;</c></summary>
    LessThan,

    /// <summary><c>(</c></summary>
    LeftParen,

    /// <summary><c>)</c></summary>
    RightParen,

    /// <summary>可选链 <c>?.</c>；记号的 <see cref="WhereToken.IsConditionalAccess"/> 为 <see langword="true"/>。</summary>
    Question,

    /// <summary><c>.</c></summary>
    Dot,

    /// <summary><c>,</c>，用于参数分隔。</summary>
    Comma,

    /// <summary>行尾。</summary>
    EndOfInput,
}

/// <summary>
/// <c>where</c> 表达式词法分析器（TODO.md §4.3）。
/// </summary>
/// <remarks>
/// 手写扫描，只接受表达式需要的记号：标识符、数字、字符串、比较与逻辑运算符、括号、
/// <c>.</c> / <c>?.</c> / <c>,</c>。不识别 <c>::</c>：<c>where</c> 的右操作数不允许引用变量系统的
/// <c>$Group::Section.Key</c>（TODO.md §4.3.2），因此 <c>:</c> 一律视为非法字符。
/// <para/>
/// 字符串字面量支持 <c>\"</c>、<c>\\</c>、<c>\n</c>、<c>\t</c>、<c>\r</c> 转义；未闭合的字符串、
/// 单独的 <c>&amp;</c> / <c>|</c> / <c>=</c> 与其它未知字符都返回失败并给出出错偏移。
/// </remarks>
public static class WhereTokenizer
{
    /// <summary>
    /// 把表达式文本拆分为记号序列。
    /// </summary>
    /// <param name="text">表达式文本。</param>
    /// <param name="tokens">成功时的记号序列，末尾恒为 <see cref="WhereTokenKind.EndOfInput"/>。</param>
    /// <param name="errorOffset">失败时出错字符在表达式内的偏移。</param>
    /// <param name="errorMessage">失败时的错误描述。</param>
    public static bool TryTokenize(string text, out IReadOnlyList<WhereToken> tokens, out int errorOffset, out string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<WhereToken> result = [];
        int position = 0;

        while (position < text.Length)
        {
            char current = text[position];
            if (char.IsWhiteSpace(current))
            {
                position++;
                continue;
            }

            int start = position;
            switch (current)
            {
                case '&':
                    if (TryConsumeDouble(text, position, '&'))
                    {
                        result.Add(new(WhereTokenKind.And, "&&", start, position + 2));
                        position += 2;
                        continue;
                    }

                    return Fail(out tokens, out errorOffset, out errorMessage, start, "'&' 需要成对出现，写成 '&&'");

                case '|':
                    if (TryConsumeDouble(text, position, '|'))
                    {
                        result.Add(new(WhereTokenKind.Or, "||", start, position + 2));
                        position += 2;
                        continue;
                    }

                    return Fail(out tokens, out errorOffset, out errorMessage, start, "'|' 需要成对出现，写成 '||'");

                case '!' when position + 1 < text.Length && text[position + 1] is '=':
                    result.Add(new(WhereTokenKind.NotEqual, "!=", start, position + 2));
                    position += 2;
                    continue;

                case '!':
                    result.Add(new(WhereTokenKind.Not, "!", start, position + 1));
                    position++;
                    continue;

                case '=':
                    if (TryConsumeDouble(text, position, '='))
                    {
                        result.Add(new(WhereTokenKind.Equal, "==", start, position + 2));
                        position += 2;
                        continue;
                    }

                    return Fail(out tokens, out errorOffset, out errorMessage, start, "比较相等应写成 '=='");

                case '>' when position + 1 < text.Length && text[position + 1] is '=':
                    result.Add(new(WhereTokenKind.GreaterOrEqual, ">=", start, position + 2));
                    position += 2;
                    continue;

                case '>':
                    result.Add(new(WhereTokenKind.GreaterThan, ">", start, position + 1));
                    position++;
                    continue;

                case '<' when position + 1 < text.Length && text[position + 1] is '=':
                    result.Add(new(WhereTokenKind.LessOrEqual, "<=", start, position + 2));
                    position += 2;
                    continue;

                case '<':
                    result.Add(new(WhereTokenKind.LessThan, "<", start, position + 1));
                    position++;
                    continue;

                case '(':
                    result.Add(new(WhereTokenKind.LeftParen, "(", start, position + 1));
                    position++;
                    continue;

                case ')':
                    result.Add(new(WhereTokenKind.RightParen, ")", start, position + 1));
                    position++;
                    continue;

                case ',':
                    result.Add(new(WhereTokenKind.Comma, ",", start, position + 1));
                    position++;
                    continue;

                case '.':
                    result.Add(Dot(text, start, position + 1, isConditionalAccess: false));
                    position++;
                    continue;

                case '?':
                    // 可选链 ?. 是一个 token；单独的 '?' 不是合法语法（见 WhereTokenKind.Question 的说明）。
                    if (position + 1 < text.Length && text[position + 1] is '.')
                    {
                        result.Add(Dot(text, start, position + 2, isConditionalAccess: true));
                        position += 2;
                        continue;
                    }

                    return Fail(out tokens, out errorOffset, out errorMessage, start, "可选链应写成 '?.'");

                case '"':
                {
                    if (TryReadString(text, ref position, out string value) is false)
                        return Fail(out tokens, out errorOffset, out errorMessage, start, "字符串字面量未闭合");

                    result.Add(new(WhereTokenKind.String, value, start, position));
                    continue;
                }

                case '$':
                {
                    // `where` 里唯一合法的 $ 形式是节名引用 `$Section.Id`（TODO.md §4.3.3）：
                    // `$` 之后必须是 `Section`，紧跟 `.` 与 `Id`。其它 $ 一律拒绝——
                    // 变量系统的 `$Group::Section.Key` 不允许出现在 where 里（TODO.md §4.3.2）。
                    position++; // 跳过 '$'
                    int nameStart = position;
                    while (position < text.Length && IsIdentifierPart(text[position]))
                        position++;

                    string sectionName = text[nameStart..position];
                    if (string.Equals(sectionName, "Section", StringComparison.OrdinalIgnoreCase) is false)
                    {
                        return Fail(out tokens, out errorOffset, out errorMessage, start,
                            "'where' 表达式里只支持节名引用 '$Section.Id'");
                    }

                    // 紧跟 '.'
                    if (position >= text.Length || text[position] is not '.')
                    {
                        return Fail(out tokens, out errorOffset, out errorMessage, start,
                            "节名引用应写成 '$Section.Id'（缺少 '.'）");
                    }

                    position++; // 跳过 '.'

                    int idStart = position;
                    while (position < text.Length && IsIdentifierPart(text[position]))
                        position++;

                    string idName = text[idStart..position];
                    if (string.Equals(idName, "Id", StringComparison.OrdinalIgnoreCase) is false)
                    {
                        return Fail(out tokens, out errorOffset, out errorMessage, start,
                            "节名引用应写成 '$Section.Id'（'.' 之后只能是 'Id'）");
                    }

                    // 整个 `$Section.Id` 折叠成一个记号，文本保存以 `$` 起的原文。
                    result.Add(new(WhereTokenKind.SectionId, text[start..position], start, position));
                    continue;
                }
            }

            if (current is '-' || char.IsAsciiDigit(current))
            {
                // 只有 '-' 紧跟数字才是数字字面量；单独的 '-' 落到下面的"未知字符"分支。
                bool isNegativeNumber = current is '-' && position + 1 < text.Length && char.IsAsciiDigit(text[position + 1]);
                if (isNegativeNumber || char.IsAsciiDigit(current))
                {
                    if (TryReadNumber(text, ref position) is false)
                        return Fail(out tokens, out errorOffset, out errorMessage, start, "非法的数字字面量");

                    result.Add(new(WhereTokenKind.Number, text[start..position], start, position));
                    continue;
                }
            }

            if (IsIdentifierStart(current))
            {
                while (position < text.Length && IsIdentifierPart(text[position]))
                    position++;

                string identifier = text[start..position];

                // 布尔字面量（大小写不敏感）：true / false 不是属性名，而是有值的字面量。
                if (string.Equals(identifier, "true", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new(WhereTokenKind.Boolean, "true", start, position));
                    continue;
                }

                if (string.Equals(identifier, "false", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new(WhereTokenKind.Boolean, "false", start, position));
                    continue;
                }

                result.Add(new(WhereTokenKind.Identifier, identifier, start, position));
                continue;
            }

            return Fail(out tokens, out errorOffset, out errorMessage, start,
                $"where 表达式不支持字符 '{current}'");
        }

        result.Add(new(WhereTokenKind.EndOfInput, string.Empty, text.Length, text.Length));
        tokens = result;
        errorOffset = -1;
        errorMessage = string.Empty;
        return true;
    }

    private static bool IsIdentifierStart(char value)
        => value is '_' || char.IsLetter(value);

    /// <summary>
    /// 构造 <see cref="WhereTokenKind.Dot"/> 记号。
    /// </summary>
    /// <remarks>
    /// 注意不要在 <c>case</c> 标签里直接写 <c>new(..., IsConditionalAccess: true)</c>：目标类型推导出的
    /// 构造函数绑定不会带上命名实参的校验，容易把可选实参位置写错。集中在这里构造，语义更清楚。
    /// </remarks>
    private static WhereToken Dot(string text, int start, int end, bool isConditionalAccess)
        => new(WhereTokenKind.Dot, text[start..end], start, end, isConditionalAccess);

    private static bool IsIdentifierPart(char value)
        => value is '_' or '-' || char.IsLetterOrDigit(value);

    private static bool TryConsumeDouble(string text, int position, char expected)
        => position + 1 < text.Length && text[position + 1] == expected;

    private static bool TryReadNumber(string text, ref int position)
    {
        if (text[position] is '-')
        {
            position++;
            if (position >= text.Length || char.IsAsciiDigit(text[position]) is false)
                return false;
        }

        while (position < text.Length && char.IsAsciiDigit(text[position]))
            position++;

        if (position < text.Length && text[position] is '.')
        {
            position++;
            while (position < text.Length && char.IsAsciiDigit(text[position]))
                position++;
        }

        return true;
    }

    private static bool TryReadString(string text, ref int position, out string value)
    {
        StringBuilder builder = new();
        position++; // 跳过起始引号

        while (position < text.Length)
        {
            char current = text[position];
            switch (current)
            {
                case '"':
                    position++;
                    value = builder.ToString();
                    return true;

                case '\\' when position + 1 < text.Length:
                    position++;
                    char escaped = text[position];
                    builder.Append(escaped switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => escaped,
                    });
                    position++;
                    continue;

                default:
                    builder.Append(current);
                    position++;
                    continue;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool Fail(
        out IReadOnlyList<WhereToken> tokens,
        out int errorOffset,
        out string errorMessage,
        int offset,
        string message)
    {
        tokens = [];
        errorOffset = offset;
        errorMessage = message;
        return false;
    }
}
