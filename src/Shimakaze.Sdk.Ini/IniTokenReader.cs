using System.Collections;
using System.Text;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Ini Token 读取器
/// </summary>
/// <param name="textReader"></param>
/// <param name="leaveOpen"></param>
public sealed class IniTokenReader(TextReader textReader, bool leaveOpen = false) : IIniTokenReader
{
    private readonly StringBuilder _buffer = new();

    private IEnumerable<IIniToken> ReadAll()
    {
        while (textReader.Read() is int rawChar and not -1)
        {
            char ch = (char)rawChar;

            IIniToken? token = ch switch
            {
                // 空白
                '\r' => IniTokenTools.CR,
                '\n' => IniTokenTools.LF,
                ' ' => IniTokenTools.SPACE,
                '\t' => IniTokenTools.TAB,
                // 符号
                ';' => IniTokenTools.SEMI,
                '=' => IniTokenTools.EQ,
                '[' => IniTokenTools.BeginBracket,
                ']' => IniTokenTools.EndBracket,
                // 其他
                _ => default
            };
            if (token is not null)
            {
                if (Flush() is IIniToken iniToken)
                    yield return iniToken;
                yield return token;
            }
            else
            {
                _buffer.Append(ch);
            }
        }
        if (Flush() is IIniToken iniToken1)
            yield return iniToken1;

        yield return IniTokenTools.EOF;

        IIniToken? Flush()
        {
            if (_buffer.Length is 0)
                return null;

            string value = _buffer.ToString();
            _buffer.Clear();
            return IniTokenTools.Value(value);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<IIniToken> GetEnumerator() => ReadAll().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!leaveOpen)
            textReader.Dispose();
    }
}
