using System.Collections;
using System.Text;

namespace Shimakaze.Sdk.Inilyn.Lexer;

/// <summary>
/// 对 INI 文本进行细粒度词法分析。
/// </summary>
/// <remarks>
/// <para>
/// 该分析器基于 <see cref="TextReader"/> 流式读取，将输入拆分为最小记号序列：
/// </para>
/// <list type="bullet">
///   <item><description>方括号：<see cref="IniTokenType.LeftBracket"/>（<c>[</c>）、<see cref="IniTokenType.RightBracket"/>（<c>]</c>）。</description></item>
///   <item><description>分隔符：<see cref="IniTokenType.EqualSign"/>（<c>=</c>）、<see cref="IniTokenType.Colon"/>（<c>:</c>）、<see cref="IniTokenType.Comma"/>（<c>,</c>）。</description></item>
///   <item><description>标记：<see cref="IniTokenType.Comment"/>（<c>;</c>）、<see cref="IniTokenType.DocComment"/>（<c>;;;</c>，作为单个记号）。</description></item>
///   <item><description><see cref="IniTokenType.Whitespace"/>：连续空白字符（空格、制表符）。</description></item>
///   <item><description><see cref="IniTokenType.Text"/>：连续的非特殊、非空白、非换行字符片段。</description></item>
///   <item><description><see cref="IniTokenType.Newline"/>：换行符（<c>\n</c>、<c>\r\n</c> 或 <c>\r</c>）。</description></item>
///   <item><description><see cref="IniTokenType.EndOfFile"/>：文件结束。</description></item>
/// </list>
/// </remarks>
/// <param name="reader">要分析的源文本读取器。</param>
public sealed class IniLexer(TextReader reader) : IEnumerable<IniToken>
{
    /// <summary>分隔符到记号类型的映射（单字符记号）。</summary>
    private static readonly Dictionary<int, IniTokenType> SimpleTokens = new()
    {
        ['['] = IniTokenType.LeftBracket,
        [']'] = IniTokenType.RightBracket,
        ['='] = IniTokenType.EqualSign,
        [':'] = IniTokenType.Colon,
        [','] = IniTokenType.Comma,
    };

    private readonly TextReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    private readonly Queue<int> _pushedBack = new();
    private int _current;
    private int _line = 1;
    private int _column = 1;
    private int _offset;
    private bool _afterEqualSign;

    /// <summary>
    /// 对给定的 INI 字符串内容进行词法分析。
    /// </summary>
    /// <param name="content">INI 文本内容。</param>
    /// <returns>按出现顺序排列的 <see cref="IniToken"/> 序列，以 <see cref="IniTokenType.EndOfFile"/> 结尾。</returns>
    public static IEnumerable<IniToken> Tokenize(string content)
    {
        using StringReader reader = new(content);
        return new IniLexer(reader);
    }

    /// <summary>
    /// 允许从字符串隐式创建词法分析器，便于 foreach 直接遍历：<c>foreach (var t in ini)</c>。
    /// </summary>
    /// <param name="content">INI 文本内容。</param>
    public static implicit operator IniLexer(string content)
    {
        using StringReader reader = new(content);
        return new IniLexer(reader);
    }

    /// <summary>
    /// 从底层 <see cref="TextReader"/> 流式读取并进行词法分析。
    /// </summary>
    /// <returns>按出现顺序排列的 <see cref="IniToken"/> 序列，以 <see cref="IniTokenType.EndOfFile"/> 结尾。</returns>
    public IEnumerable<IniToken> Tokenize()
    {
        _current = _reader.Read();
        while (_current != -1)
        {
            yield return ReadToken();
        }

        yield return MakeToken(_line, _column, _offset, IniTokenType.EndOfFile, string.Empty);
    }

    /// <inheritdoc />
    public IEnumerator<IniToken> GetEnumerator() => Tokenize().GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IniToken ReadToken()
    {
        int line = _line;
        int column = _column;
        int offset = _offset;

        // 等号右侧：读取整行作为值
        if (_afterEqualSign)
        {
            _afterEqualSign = false;
            return MakeToken(line, column, offset, IniTokenType.Text, ConsumeToLineEnd());
        }

        if (SimpleTokens.TryGetValue(_current, out var type))
        {
            char text = (char)_current;
            Advance();
            // 等号后面进入值模式
            if (_current == -1 && type == IniTokenType.EqualSign)
                _afterEqualSign = true;
            else if (type == IniTokenType.EqualSign)
                _afterEqualSign = true;
            return MakeToken(line, column, offset, type, text.ToString());
        }

        return _current switch
        {
            ';' => ReadSemicolon(line, column, offset),
            '#' => ReadPreprocessorDirective(line, column, offset),
            ' ' or '\t' => MakeToken(line, column, offset, IniTokenType.Whitespace, ConsumeWhile(IsWhitespace)),
            '\r' or '\n' => MakeToken(line, column, offset, IniTokenType.Newline, ConsumeNewline()),
            _ => MakeToken(line, column, offset, IniTokenType.Text, ConsumeWhile(IsStringChar)),
        };
    }

    private IniToken ReadSemicolon(int line, int column, int offset)
    {
        if (Peek(1) == ';' && Peek(2) == ';')
        {
            Advance();
            Advance();
            Advance();
            // 读取文档注释内容直到换行或 EOF
            StringBuilder builder = new(";;;");
            while (_current != -1 && _current != '\r' && _current != '\n')
            {
                builder.Append((char)_current);
                Advance();
            }
            return MakeToken(line, column, offset, IniTokenType.DocComment, builder.ToString());
        }

        // 读取行注释内容直到换行或 EOF
        StringBuilder commentBuilder = new(";");
        Advance();
        while (_current != -1 && _current != '\r' && _current != '\n')
        {
            commentBuilder.Append((char)_current);
            Advance();
        }
        return MakeToken(line, column, offset, IniTokenType.Comment, commentBuilder.ToString());
    }

    private IniToken ReadPreprocessorDirective(int line, int column, int offset)
    {
        StringBuilder builder = new();
        builder.Append((char)_current); // '#'
        Advance();

        // 读取指令内容直到换行或 EOF
        while (_current != -1 && _current != '\r' && _current != '\n')
        {
            builder.Append((char)_current);
            Advance();
        }

        return MakeToken(line, column, offset, IniTokenType.PreprocessorDirective, builder.ToString());
    }

    private static IniToken MakeToken(int line, int column, int offset, IniTokenType type, string text)
        => new(type, line, column, offset, text);

    private void Advance()
    {
        int ch = _current;
        if (ch == '\n')
        {
            _line++;
            _column = 1;
        }
        else if (ch == '\r')
        {
            if (Peek(1) != '\n')
            {
                _line++;
            }

            _column = 1;
        }
        else
        {
            _column++;
        }

        _offset++;
        _current = _pushedBack.Count > 0 ? _pushedBack.Dequeue() : _reader.Read();
    }

    private int Peek(int ahead)
    {
        while (_pushedBack.Count < ahead)
        {
            _pushedBack.Enqueue(_reader.Read());
        }

        return _pushedBack.ElementAt(ahead - 1);
    }

    private string ConsumeWhile(Func<int, bool> predicate)
    {
        StringBuilder builder = new();
        while (_current != -1 && predicate(_current))
        {
            builder.Append((char)_current);
            Advance();
        }

        return builder.ToString();
    }

    private string ConsumeToLineEnd()
    {
        StringBuilder builder = new();
        while (_current != -1 && _current != '\r' && _current != '\n' && _current != ';')
        {
            builder.Append((char)_current);
            Advance();
        }
        // 去除首尾空白
        return builder.ToString().Trim();
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

    private static bool IsWhitespace(int c) => c == ' ' || c == '\t';

    private static bool IsStringChar(int c) => !SimpleTokens.ContainsKey(c) && !IsWhitespace(c) && c != ';' && c != '#' && c != '\r' && c != '\n' && c != -1;
}
