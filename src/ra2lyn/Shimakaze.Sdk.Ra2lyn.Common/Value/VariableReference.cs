namespace Shimakaze.Sdk.Ra2lyn.Common.Value;

/// <summary>
/// 属性值中一个 <c>$</c> 引用的词法识别结果（TODO.md §3.5）。
/// </summary>
/// <param name="Kind">引用的种类，见 <see cref="VariableReferenceKind"/>。</param>
/// <param name="Start">引用在属性值字符串内的起始偏移（<c>$</c> 的位置）。</param>
/// <param name="End">引用在属性值字符串内的结束偏移（不含）。</param>
/// <param name="Expression">
/// 路径表达式原文；裸占位符为 <see langword="null"/>。
/// 花括号变量的 <c>{ }</c> 已被剥掉，普通变量则是贪婪匹配得到的整段文本。
/// </param>
public sealed record class VariableReference(
    VariableReferenceKind Kind,
    int Start,
    int End,
    string? Expression)
{
    /// <summary>
    /// 引用在属性值字符串内覆盖的文本。
    /// </summary>
    public int Length
        => End - Start;

    /// <summary>
    /// 是否为"裸 <c>$</c>"占位符（<c>Key = $, D</c>，TODO.md §2）。
    /// </summary>
    public bool IsCurrentValuePlaceholder
        => Kind is VariableReferenceKind.CurrentValuePlaceholder;
}

/// <summary>
/// <c>$</c> 引用的种类（TODO.md §3.5.5）。
/// </summary>
public enum VariableReferenceKind
{
    /// <summary>
    /// 普通变量 <c>$Expr</c>：<c>$</c> 后紧跟词字符，按 <see cref="VariableReference"/> 的状态机贪婪识别。
    /// </summary>
    Variable,

    /// <summary>
    /// 花括号变量 <c>${Expr}</c>：<c>$</c> 后紧跟 <c>{</c>，一直读到匹配的 <c>}</c>。
    /// </summary>
    BracedVariable,

    /// <summary>
    /// 裸 <c>$</c>：<c>$</c> 后跟其他标点（<c>,</c> <c>=</c> <c>;</c> 空格 行尾 等），表示"当前最终值"占位符（TODO.md §2）。
    /// </summary>
    CurrentValuePlaceholder,
}

/// <summary>
/// 词法识别后的一段属性值：一段 <c>$</c> 引用，或一段不含引用的字面文本。
/// </summary>
/// <param name="Reference">引用；为 <see langword="null"/> 时表示这段是字面文本。</param>
/// <param name="Text">字面文本；引用段为 <see langword="null"/>。</param>
/// <param name="Start">在属性值字符串内的起始偏移。</param>
/// <param name="End">在属性值字符串内的结束偏移（不含）。</param>
public sealed record class ValueSegment(VariableReference? Reference, string? Text, int Start, int End);

/// <summary>
/// <c>$</c> 引用的词法分析器（TODO.md §3.5.3、§3.5.4、§3.5.5）。
/// </summary>
/// <remarks>
/// <para>
/// 只做词法：判定每个 <c>$</c> 是花括号变量、普通变量还是裸占位符，并给出普通变量的贪婪匹配边界。
/// 路径语义（四种路径形式、常量 <c>$null</c>、符号表查询）由 <see cref="ValueResolver"/> 负责。
/// </para>
/// <para>
/// 无花括号时的状态机（TODO.md §3.5.3）：
/// </para>
/// <list type="number">
/// <item>读一个"词"（字母 / 数字 / <c>_</c> / <c>-</c> / 中文等连续非标点字符）。</item>
/// <item>Group 段（还没吃过 <c>.</c>）：<c>::</c> 后跟词可以继续吃，<c>.</c> 后跟词会把状态切到 Section/Key 段。</item>
/// <item>Section/Key 段（吃过至少一个 <c>.</c>）：只允许继续吃 <c>.</c> + 词；遇到 <c>::</c> 立即停止，
/// 把 <c>::</c> 留在表达式外。</item>
/// <item>其他标点（<c>,</c> <c>=</c> <c>;</c> 空格 行尾 等）立即停止。</item>
/// </list>
/// </remarks>
public static class VariableReferenceParser
{
    private const char Marker = '$';
    private const char OpenBrace = '{';
    private const char CloseBrace = '}';

    /// <summary>
    /// 把属性值切成"引用段 / 字面段"交替的序列。
    /// </summary>
    /// <param name="value">属性原始值字符串。</param>
    /// <returns>分段序列；属性值不含 <c>$</c> 时返回单个字面段（或空序列，当值为空串时）。</returns>
    /// <exception cref="ArgumentException">属性值里存在无法识别的 <c>$</c> 用法（未配对的 <c>${</c>、<c>${}</c> 等）。</exception>
    /// <remarks>
    /// 需要拿到错误偏移的调用方应改用 <see cref="TryParse"/>，避免异常开销。
    /// </remarks>
    public static IReadOnlyList<ValueSegment> Parse(string value)
        => TryParse(value, out var segments, out string? error, out _, out _)
            ? segments!
            : throw new ArgumentException(error, nameof(value));

    /// <summary>
    /// 把属性值切成"引用段 / 字面段"交替的序列，错误以返回值报告而不抛异常。
    /// </summary>
    /// <param name="value">属性原始值字符串。</param>
    /// <param name="segments">分段结果；失败时为 <see langword="null"/>。</param>
    /// <param name="error">面向用户的错误消息；成功时为 <see langword="null"/>。</param>
    /// <param name="errorOffset">错误在属性值内的起始偏移。</param>
    /// <param name="errorLength">错误片段的长度。</param>
    /// <returns>是否成功。</returns>
    public static bool TryParse(
        string value,
        out IReadOnlyList<ValueSegment>? segments,
        out string? error,
        out int errorOffset,
        out int errorLength)
    {
        ArgumentNullException.ThrowIfNull(value);

        List<ValueSegment> result = [];
        segments = null;
        error = null;
        errorOffset = 0;
        errorLength = 1;

        int literalStart = 0;
        int index = 0;
        while (index < value.Length)
        {
            if (value[index] is not Marker)
            {
                index++;
                continue;
            }

            if (literalStart < index)
                result.Add(new(null, value[literalStart..index], literalStart, index));

            if (index + 1 >= value.Length)
            {
                // `$` 在行尾：按 §3.5.5 是裸占位符，不是错误。
                result.Add(new(new(VariableReferenceKind.CurrentValuePlaceholder, index, index + 1, null), null, index, index + 1));
                index++;
                literalStart = index;
                continue;
            }

            char next = value[index + 1];
            if (next is OpenBrace)
            {
                if (TryParseBraced(value, index, out var braced, out error, out errorOffset, out errorLength) is false)
                {
                    segments = null;
                    return false;
                }

                result.Add(new(braced!, null, braced!.Start, braced.End));
                index = braced.End;
                literalStart = index;
                continue;
            }

            if (IsWordCharacter(next) is false)
            {
                // 裸占位符：`$` 后面是 `,` / `=` / `;` / 空格 等（TODO.md §3.5.5）。
                result.Add(new(new(VariableReferenceKind.CurrentValuePlaceholder, index, index + 1, null), null, index, index + 1));
                index++;
                literalStart = index;
                continue;
            }

            int end = ScanVariableExpression(value, index + 1);
            string expression = value[(index + 1)..end];
            result.Add(new(new(VariableReferenceKind.Variable, index, end, expression), null, index, end));
            index = end;
            literalStart = index;
        }

        if (literalStart < value.Length)
            result.Add(new(null, value[literalStart..], literalStart, value.Length));

        segments = result;
        return true;
    }

    /// <summary>
    /// 判断一个字符是否属于"词"（TODO.md §3.5.3 的标识符：字母 / 数字 / <c>_</c> / <c>-</c> / 中文等非标点字符）。
    /// </summary>
    /// <remarks>
    /// 标点集合固定为 INI 里有结构意义的字符与路径分隔符：<c>, = ; [ ] ( ) { } " ' : . $ # \t</c> 与空格；
    /// 其余字符（字母、数字、<c>_</c>、<c>-</c>、中文等）都算词字符。
    /// </remarks>
    public static bool IsWordCharacter(char value)
        => value is not (',' or '=' or ';' or '[' or ']' or '(' or ')' or '{' or '}' or '"'
            or '\'' or ':' or '.' or Marker or '#' or '\t' or ' ')
        && char.IsWhiteSpace(value) is false;

    /// <summary>
    /// 判断一段文本是否符合路径表达式形状：<c>Group::Section.Key</c> / <c>Section.Key</c> / <c>Key</c> / <c>Group::Key</c>。
    /// </summary>
    /// <remarks>
    /// 只做形状校验（空段、<c>::</c> 之后重新吃 <c>.</c> 等结构性错误），不查表。
    /// </remarks>
    public static bool IsPathExpression(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        if (expression.Length is 0)
            return false;

        // 先按 `::` 拆组段，再按 `.` 拆节点段（TODO.md §3.5.1 解析优先级）。
        int doubleColon = expression.IndexOf("::", StringComparison.Ordinal);
        if (doubleColon < 0)
        {
            foreach (string part in expression.Split('.'))
            {
                if (part.Length is 0 || ContainsOnlyWordCharacters(part) is false)
                    return false;
            }

            return expression.Split('.').Length <= 2;
        }

        foreach (string groupPart in expression[..doubleColon].Split("::"))
        {
            if (groupPart.Length is 0 || ContainsOnlyWordCharacters(groupPart) is false)
                return false;
        }

        string tail = expression[(doubleColon + 2)..];
        if (tail.Length is 0 || tail.Contains("::", StringComparison.Ordinal))
            return false;

        foreach (string part in tail.Split('.'))
        {
            if (part.Length is 0 || ContainsOnlyWordCharacters(part) is false)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 从 <c>$</c> 之后开始贪婪扫描普通变量的路径表达式，返回表达式的结束偏移（不含）。
    /// </summary>
    /// <remarks>
    /// 实现 TODO.md §3.5.3 的状态机。扫描起点必须落在词字符上。
    /// </remarks>
    private static int ScanVariableExpression(string value, int start)
    {
        bool inSectionKeySegment = false;
        int position = start;

        // 第一个词。
        position = ScanWord(value, position);
        if (position == start)
            return start;

        while (position < value.Length)
        {
            // `::` 只在 Group 段合法；进入 Section/Key 段后立即停止，把 `::` 留在表达式外。
            if (value[position] is ':'
                && position + 1 < value.Length
                && value[position + 1] is ':')
            {
                if (inSectionKeySegment)
                    return position;

                int wordStart = position + 2;
                if (wordStart >= value.Length || IsWordCharacter(value[wordStart]) is false)
                    return position;

                position = ScanWord(value, wordStart);
                continue;
            }

            // `.` 把状态切到 Section/Key 段（或者停留在该段），后面必须紧跟一个词。
            if (value[position] is '.')
            {
                int wordStart = position + 1;
                if (wordStart >= value.Length || IsWordCharacter(value[wordStart]) is false)
                    return position;

                position = ScanWord(value, wordStart);
                inSectionKeySegment = true;
                continue;
            }

            break;
        }

        return position;
    }

    /// <summary>
    /// 扫描一个连续词，返回词尾之后的位置。
    /// </summary>
    private static int ScanWord(string value, int start)
    {
        int position = start;
        while (position < value.Length && IsWordCharacter(value[position]))
            position++;

        return position;
    }

    private static bool ContainsOnlyWordCharacters(string text)
    {
        if (text.Length is 0)
            return false;

        foreach (char character in text)
        {
            if (IsWordCharacter(character) is false)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 解析花括号变量 <c>${Expr}</c>：从 <c>$</c> 当前位置读到匹配的 <c>}</c>（TODO.md §3.5.4）。
    /// </summary>
    /// <remarks>
    /// 第一版不允许嵌套花括号，因此不存在"匹配"问题：只找第一个 <c>}</c>。
    /// 未配对的 <c>${</c> 与空表达式 <c>${}</c> 都当作词法错误报告，由调用方保留原始字符串。
    /// </remarks>
    private static bool TryParseBraced(
        string value,
        int dollarIndex,
        out VariableReference? reference,
        out string? error,
        out int errorOffset,
        out int errorLength)
    {
        reference = null;
        int openIndex = dollarIndex + 1;
        int closeIndex = value.IndexOf(CloseBrace, openIndex + 1);

        errorOffset = dollarIndex;
        errorLength = Math.Min(value.Length - dollarIndex, 2);

        if (closeIndex < 0)
        {
            // 报告剩余的全部文本，让诊断范围落到未配对的 `${` 起。
            errorLength = value.Length - dollarIndex;
            error = "未配对的 '${'：'${' 之后缺少 '}'";
            return false;
        }

        string expression = value[(openIndex + 1)..closeIndex];
        if (expression.Length is 0)
        {
            errorOffset = dollarIndex;
            errorLength = closeIndex + 1 - dollarIndex;
            error = "花括号变量 '${}' 的路径表达式为空";
            return false;
        }

        if (expression.Contains(OpenBrace) || expression.Contains(CloseBrace))
        {
            errorOffset = dollarIndex;
            errorLength = closeIndex + 1 - dollarIndex;
            error = "花括号变量内部不允许再出现花括号（第一版不支持嵌套）";
            return false;
        }

        reference = new(VariableReferenceKind.BracedVariable, dollarIndex, closeIndex + 1, expression);
        error = null;
        return true;
    }
}
