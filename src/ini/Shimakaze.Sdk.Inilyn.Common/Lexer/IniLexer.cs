using System.Runtime.CompilerServices;

using Shimakaze.Sdk.Inilyn.Data;
using Shimakaze.Sdk.Inilyn.Data.Lexer;

namespace Shimakaze.Sdk.Inilyn.Lexer;

/// <summary>
/// 对 INI 文本进行细粒度词法分析，并将结果持久化到数据库。
/// </summary>
/// <remarks>
/// <para>
/// 该分析器将输入字符串拆分为最小记号序列：
/// </para>
/// <list type="bullet">
///   <item><description>方括号：<see cref="IniTokenType.LeftBracket"/>（<c>[</c>）、<see cref="IniTokenType.RightBracket"/>（<c>]</c>）。</description></item>
///   <item><description>分隔符：<see cref="IniTokenType.EqualSign"/>（<c>=</c>）、<see cref="IniTokenType.Colon"/>（<c>:</c>）、<see cref="IniTokenType.Comma"/>（<c>,</c>）。</description></item>
///   <item><description>分号：<see cref="IniTokenType.Semicolon"/>（<c>;</c>）。</description></item>
///   <item><description>井号：<see cref="IniTokenType.Hash"/>（<c>#</c>）。</description></item>
///   <item><description><see cref="IniTokenType.Whitespace"/>：连续空白字符（空格、制表符）。</description></item>
///   <item><description><see cref="IniTokenType.Text"/>：连续的非特殊、非空白、非换行字符片段。</description></item>
///   <item><description><see cref="IniTokenType.Newline"/>：换行符（<c>\n</c>、<c>\r\n</c> 或 <c>\r</c>）。</description></item>
///   <item><description><see cref="IniTokenType.EndOfFile"/>：文件结束。</description></item>
/// </list>
/// </remarks>
/// <param name="dbContext">数据库上下文。</param>
public sealed class IniLexer(IniDbContext dbContext)
{
    private string _source = string.Empty;
    private int _length;
    private int _position;
    private int _current;
    private int _line;
    private int _column;

    /// <summary>
    /// 对源文本进行词法分析并将记号写入数据库。
    /// </summary>
    /// <param name="source">要分析的源文本。</param>
    /// <param name="documentId">所属文档的 Id。</param>
    public void Tokenize(string source, Guid documentId)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _length = source.Length;

        List<IniToken> tokens = [];

        _current = Peek(0);
        int order = 0;
        while (_current != -1)
        {
            tokens.Add(ReadToken(documentId, order++));
        }

        tokens.Add(MakeToken(documentId, order, IniTokenType.EndOfFile, string.Empty, _line, _column, _line, _column));

        dbContext.Tokens.AddRange(tokens);
        dbContext.SaveChanges();
    }

    private IniToken ReadToken(Guid documentId, int order)
    {
        int line = _line;
        int column = _column;

        switch (_current)
        {
            case '[':
                Advance();
                return MakeToken(documentId, order, IniTokenType.LeftBracket, "[", line, column, _line, _column);
            case ']':
                Advance();
                return MakeToken(documentId, order, IniTokenType.RightBracket, "]", line, column, _line, _column);
            case '=':
                Advance();
                return MakeToken(documentId, order, IniTokenType.EqualSign, "=", line, column, _line, _column);
            case ':':
                Advance();
                return MakeToken(documentId, order, IniTokenType.Colon, ":", line, column, _line, _column);
            case ',':
                Advance();
                return MakeToken(documentId, order, IniTokenType.Comma, ",", line, column, _line, _column);
            case ';':
                Advance();
                return MakeToken(documentId, order, IniTokenType.Semicolon, ";" + ConsumeToLineEnd(), line, column, _line, _column);
            case '#':
                Advance();
                return MakeToken(documentId, order, IniTokenType.Hash, "#" + ConsumeToLineEnd(), line, column, _line, _column);
            case ' ' or '\t':
                return MakeToken(documentId, order, IniTokenType.Whitespace, ConsumeWhile(IsWhitespace), line, column, _line, _column);
            case '\r' or '\n':
                return MakeToken(documentId, order, IniTokenType.Newline, ConsumeNewline(), line, column, _line, _column);
            default:
                return MakeToken(documentId, order, IniTokenType.Text, ConsumeWhile(IsTextChar), line, column, _line, _column);
        }
    }

    /// <summary>
    /// 创建一个词法记号。
    /// </summary>
    /// <param name="documentId">所属文档的 Id。</param>
    /// <param name="order">记号在文档中的顺序（从 0 开始）。</param>
    /// <param name="type">记号类型。</param>
    /// <param name="text">记号的词素文本。</param>
    /// <param name="startLine">记号起始行号（零基）。</param>
    /// <param name="startColumn">记号起始列号（零基）。</param>
    /// <param name="endLine">记号结束行号（零基）。</param>
    /// <param name="endColumn">记号结束列号（零基）。</param>
    /// <returns>新创建的记号。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IniToken MakeToken(Guid documentId, int order, IniTokenType type, string text, int startLine, int startColumn, int endLine, int endColumn) => new()
    {
        Id = Guid.NewGuid(),
        DocumentId = documentId,
        Order = order,
        Type = type,
        Position = new()
        {
            Start = new()
            {
                Line = (uint)startLine,
                Character = (uint)startColumn
            },
            End = new()
            {
                Line = (uint)endLine,
                Character = (uint)endColumn
            }
        },
        Text = text,
    };

    private string ConsumeWhile(Func<int, bool> predicate)
    {
        int start = _position;
        while (_current != -1 && predicate(_current))
        {
            Advance();
        }

        int length = _position - start;
        return length > 0 ? _source.Substring(start, length) : string.Empty;
    }

    private string ConsumeToLineEnd()
    {
        int start = _position;
        while (_current is not -1 and not '\r' and not '\n' and not ';')
        {
            Advance();
        }

        int length = _position - start;
        if (length == 0)
            return string.Empty;

        var slice = _source.AsSpan(start, length);
        return Trim(slice);

        static string Trim(ReadOnlySpan<char> s)
        {
            int trimStart = 0;
            int trimEnd = s.Length - 1;
            while (trimStart <= trimEnd && IsWhitespace(s[trimStart]))
                trimStart++;
            while (trimEnd >= trimStart && IsWhitespace(s[trimEnd]))
                trimEnd--;
            return s.Slice(trimStart, trimEnd - trimStart + 1).ToString();
        }
    }

    private string ConsumeNewline()
    {
        if (_current == '\r')
        {
            if (Peek(1) == '\n')
            {
                Advance();
                Advance();
                return "\r\n";
            }

            Advance();
            return "\r";
        }

        Advance();
        return "\n";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance()
    {
        int ch = _current;
        if (ch == '\n')
        {
            _line++;
            _column = 0;
        }
        else if (ch == '\r')
        {
            if (Peek(1) != '\n')
            {
                _line++;
            }

            _column = 0;
        }
        else
        {
            _column++;
        }

        _position++;
        _current = Peek(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Peek(int ahead)
    {
        int index = _position + ahead;
        return index < _length ? _source[index] : -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWhitespace(int c) => c is ' ' or '\t';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTextChar(int c) =>
        c is not '[' and not ']' and not '=' and not ':' and not ','
        and not ';' and not '#' and not ' ' and not '\t'
        and not '\r' and not '\n' and not -1;
}