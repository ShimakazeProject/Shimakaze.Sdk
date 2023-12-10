using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
    protected readonly BucketStack _depths = new();

    /// <summary>
    /// 字符暂存区
    /// </summary>
    protected StringBuilder Buffer => _depths.Buffer;
    private bool _disposedValue;

    /// <summary>
    /// 将 <see cref="Buffer"/> 内容输出为 <see cref="IniToken"/>
    /// </summary>
    /// <param name="result"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    protected virtual bool FlushBuffer([NotNullWhen(true)] out IniToken? result, int type = IniTokenType.Unknown)
    {
        result = default;

        switch (_depths.Current)
        {
            // Flush Value
            case '[' when _depths.Pop(out var value):
                result = new IniToken(IniTokenType.Section, value, IgnoreLevel is >= IniTokenIgnoreLevel.White);
                break;
            case '=' when _depths.Pop(out var value):
                result = new IniToken(IniTokenType.Value, value, IgnoreLevel is >= IniTokenIgnoreLevel.White);
                break;
            default:
                if (Buffer is not { Length: 0 })
                {
                    // 不知道是什么东西
                    result = new IniToken(type, Buffer.ToString(), IgnoreLevel is >= IniTokenIgnoreLevel.White);
                    Buffer.Clear();
                }
                break;
        }

        return result is not null;
    }

    private IEnumerable<IniToken> ReadAllInternal()
    {
        IniToken? token;
        while (BaseReader.Peek() is not -1)
        {
            char ch = (char)BaseReader.Read();

            // 字符匹配
            switch (ch)
            {
                // 行尾匹配
                case '\r':
                    {
                        // Flush
                        if (FlushBuffer(out token))
                            yield return token.Value;
                        if (IgnoreLevel < IniTokenIgnoreLevel.White)
                            yield return new(IniTokenType.CR);
                        break;
                    }
                case '\n':
                    {
                        // Flush
                        if (FlushBuffer(out token))
                            yield return token.Value;
                        if (IgnoreLevel < IniTokenIgnoreLevel.White)
                            yield return new(IniTokenType.LF);
                        break;
                    }
                // 空格
                case ' ':
                    {
                        if (_depths.Empty && IgnoreLevel < IniTokenIgnoreLevel.White)
                            yield return new(IniTokenType.SPACE);
                        break;
                    }
                // 横向制表符
                case '\t':
                    {
                        if (_depths.Empty && IgnoreLevel < IniTokenIgnoreLevel.White)
                            yield return new(IniTokenType.TAB);
                        break;
                    }
                // 行内注释
                case ';':
                    {
                        // Flush
                        if (FlushBuffer(out token))
                            yield return token.Value;
                        if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                            yield return new(IniTokenType.SEMI);

                        yield return new IniToken(IniTokenType.Comment, BaseReader.ReadLine() ?? string.Empty, IgnoreLevel is >= IniTokenIgnoreLevel.White);
                        break;
                    }
                // 匹配尾中括号
                case ']':
                    {
                        // Flush
                        if (FlushBuffer(out token))
                            yield return token.Value;
                        if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                            yield return new(IniTokenType.END_BRACKET);
                        break;
                    }
                // 匹配首中括号
                case '[':
                    {
                        // Flush
                        if (FlushBuffer(out token))
                            yield return token.Value;
                        if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                            yield return new(IniTokenType.START_BRACKET);

                        _depths.Push(ch);
                        break;
                    }
                case '=':
                    {
                        // Flush
                        if (FlushBuffer(out token, IniTokenType.Key))
                            yield return token.Value;
                        if (IgnoreLevel < IniTokenIgnoreLevel.NonValue)
                            yield return new(IniTokenType.EQ);

                        _depths.Push(ch);
                        break;
                    }
                default:
                    {
                        Buffer.Append(ch);
                        break;
                    }
            }
        }
        if (FlushBuffer(out token))
            yield return token.Value;
        if (IgnoreLevel < IniTokenIgnoreLevel.White)
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