using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Lexing;

/// <summary>
/// INI 词法分析器：把源文本拆分为最小记号序列。
/// </summary>
/// <remarks>
/// <para>
/// 记号集合：方括号 <c>[</c> <c>]</c>、分隔符 <c>=</c> <c>:</c> <c>,</c>、注释 <c>#</c> <c>;</c>、
/// 连续的空白片段、连续的文本片段、换行符与文件结束。
/// </para>
/// <para>
/// <c>#</c> 与 <c>;</c> 在 MVP 中一律作为行注释处理：记号文本从该符号开始一直延伸到行尾，
/// 尾部空白被裁剪；整行剩余的空白与换行仍会作为独立记号输出，因此记号序列始终覆盖全文。
/// </para>
/// </remarks>
public static class IniLexer
{
    /// <summary>
    /// 对源文本进行词法分析。
    /// </summary>
    /// <param name="text">INI 源文本。</param>
    /// <param name="path">源文件标识，用于填充记号位置。</param>
    /// <returns>按出现顺序排列的记号序列，末尾必有且仅有一个 <see cref="IniTokenType.EndOfFile"/>。</returns>
    public static IReadOnlyList<IniToken> Tokenize(string text, string path)
    {
        List<IniToken> tokens = [];
        int position = 0;

        while (position < text.Length)
        {
            IniToken token = ReadToken(text, path, ref position);
            tokens.Add(token);
        }

        tokens.Add(new(IniTokenType.EndOfFile, string.Empty, new(path, text.Length, text.Length)));
        return tokens;
    }

    private static IniToken ReadToken(string text, string path, ref int position)
    {
        int start = position;

        return text[start] switch
        {
            '[' => MakeToken(IniTokenType.LeftBracket, text, path, ref position, start, 1),
            ']' => MakeToken(IniTokenType.RightBracket, text, path, ref position, start, 1),
            '=' => MakeToken(IniTokenType.EqualSign, text, path, ref position, start, 1),
            ':' => MakeToken(IniTokenType.Colon, text, path, ref position, start, 1),
            ',' => MakeToken(IniTokenType.Comma, text, path, ref position, start, 1),
            '#' => ReadComment(IniTokenType.Hash, text, path, ref position, start),
            ';' => ReadComment(IniTokenType.Semicolon, text, path, ref position, start),
            '\r' or '\n' => ReadNewline(text, path, ref position, start),
            ' ' or '\t' => ReadRun(IniTokenType.Whitespace, text, path, ref position, start),
            _ => ReadRun(IniTokenType.Text, text, path, ref position, start),
        };
    }

    private static IniToken MakeToken(IniTokenType type, string text, string path, ref int position, int start, int length)
    {
        position = start + length;
        return new(type, text[start..position], new(path, start, position));
    }

    private static IniToken ReadRun(IniTokenType type, string text, string path, ref int position, int start)
    {
        position = start;
        while (position < text.Length && IsRunChar(type, text[position]))
            position++;

        return new(type, text[start..position], new(path, start, position));
    }

    private static IniToken ReadComment(IniTokenType type, string text, string path, ref int position, int start)
    {
        // 注释从符号开始延伸到行尾；尾部空白被裁剪（裁剪不进一个换行）。
        position = start + 1;
        while (position < text.Length && text[position] is not ('\r' or '\n'))
            position++;

        int contentEnd = position;
        while (contentEnd > start + 1 && IsBlank(text[contentEnd - 1]))
            contentEnd--;

        return new(type, text[start..contentEnd], new(path, start, position));
    }

    private static IniToken ReadNewline(string text, string path, ref int position, int start)
    {
        int length = text[start] is '\r' && start + 1 < text.Length && text[start + 1] is '\n'
            ? 2
            : 1;
        position = start + length;
        return new(IniTokenType.Newline, text[start..position], new(path, start, position));
    }

    private static bool IsRunChar(IniTokenType type, char value)
        => type switch
        {
            IniTokenType.Whitespace => IsBlank(value),
            _ => IsTextChar(value),
        };

    private static bool IsBlank(char value)
        => value is ' ' or '\t';

    private static bool IsTextChar(char value)
        => value is not ('[' or ']' or '=' or ':' or ',' or ';' or '#' or ' ' or '\t' or '\r' or '\n');
}
