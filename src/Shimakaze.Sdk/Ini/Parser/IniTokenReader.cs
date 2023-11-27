using System.Collections;
using System.Text;

namespace Shimakaze.Sdk.Ini.Parser;

/// <summary>
/// 语法树抽象
/// </summary>
/// <param name="textReader"></param>
/// <param name="ignore"></param>
/// <param name="leaveOpen"></param>
public class IniTokenReader(TextReader textReader, IniTokenIgnoreLevel ignore = IniTokenIgnoreLevel.NonValue, bool leaveOpen = false) : IEnumerable<IniToken>, IDisposable
{
    /// <summary>
    /// leave Open
    /// </summary>
    protected bool _leaveOpen = leaveOpen;

    /// <summary>
    /// 基础流
    /// </summary>
    public TextReader BaseReader { get; } = textReader;
    /// <summary>
    /// 忽略等级
    /// </summary>
    public IniTokenIgnoreLevel IgnoreLevel { get; } = ignore;

    /// <summary>
    /// 括号栈
    /// </summary>
    protected readonly Stack<(char Start, StringBuilder Buffer)> _depths = new();

    /// <summary>
    /// 字符暂存区
    /// </summary>
    protected StringBuilder _buffer = new();
    private bool _disposedValue;

    /// <summary>
    /// 扩展
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    protected virtual IniToken? Ext(char ch) => default;

    private IEnumerable<IniToken> ReadAllInternal()
    {
        while (BaseReader.Peek() is not -1)
        {
            char ch = (char)BaseReader.Read();

            // 字符匹配
            switch (ch)
            {
                // 行尾匹配
                case '\r':
                    // Flush
                    if (_buffer.Length is not 0)
                    {
                        if (_depths.TryPeek(out var depth) && depth.Start is '=')
                        {
                            (_, StringBuilder lastBuffer) = _depths.Pop();
                            yield return new IniToken(IniTokenType.Value, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                            _buffer = lastBuffer;

                        }
                        else
                        {
                            yield return new IniToken(IniTokenType.Unknown, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                            _buffer.Clear();
                        }
                    }
                    if (IgnoreLevel < IniTokenIgnoreLevel.White)
                        yield return new(IniTokenType.CR);
                    break;
                case '\n':
                    // Flush
                    if (_buffer.Length is not 0)
                    {
                        if (_depths.TryPeek(out var depth) && depth.Start is '=')
                        {
                            (_, StringBuilder lastBuffer) = _depths.Pop();
                            yield return new IniToken(IniTokenType.Value, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                            _buffer = lastBuffer;
                        }
                        else
                        {
                            yield return new IniToken(IniTokenType.Unknown, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                            _buffer.Clear();
                        }
                    }
                    if (IgnoreLevel < IniTokenIgnoreLevel.White)
                        yield return new(IniTokenType.LF);
                    break;
                // 行内注释
                case ';':
                    // Flush
                    if (_buffer.Length is not 0)
                    {
                        yield return new IniToken(IniTokenType.Unknown, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                        _buffer.Clear();
                    }
                    if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                        yield return new(IniTokenType.SEMI);

                    yield return new IniToken(IniTokenType.Comment, BaseReader.ReadLine() ?? string.Empty, IgnoreLevel is >= IniTokenIgnoreLevel.White);
                    break;
                // 匹配尾中括号
                case ']':
                    // Flush
                    if (_buffer.Length is not 0)
                    {
                        if (_depths.TryPeek(out var depth) && depth.Start is '[')
                        {
                            (_, StringBuilder lastBuffer) = _depths.Pop();
                            yield return new IniToken(IniTokenType.Section, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                            _buffer = lastBuffer;
                        }
                        else
                        {
                            yield return new IniToken(IniTokenType.Unknown, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                            _buffer.Clear();
                        }
                    }
                    if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                        yield return new(IniTokenType.END_BRACKET);
                    break;
                // 匹配首中括号
                case '[':
                    // Flush
                    if (_buffer.Length is not 0)
                    {
                        yield return new IniToken(IniTokenType.Unknown, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                        _buffer.Clear();
                    }
                    if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                        yield return new(IniTokenType.START_BRACKET);

                    _depths.Push((ch, _buffer));
                    _buffer = new();
                    break;
                case '=':
                    // Flush
                    if (_buffer.Length is not 0)
                    {
                        yield return new IniToken(IniTokenType.Key, _buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                        _buffer.Clear();
                    }
                    if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                        yield return new(IniTokenType.EQ);

                    _depths.Push((ch, _buffer));
                    _buffer = new();
                    break;
                default:
                    if (Ext(ch) is IniToken token)
                        yield return token;
                    else
                        _buffer.Append(ch);
                    break;
            }
        }
        if (IgnoreLevel < IniTokenIgnoreLevel.White && BaseReader.Peek() is -1)
            yield return new(IniTokenType.EOF);
    }

    /// <inheritdoc/>
    public IEnumerator<IniToken> GetEnumerator() => ReadAllInternal().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;
        
        _disposedValue = true;
        if (disposing)
        {
            if (!_leaveOpen)
                BaseReader.Dispose();
        }

    }

    // ~IniTokenReader()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}