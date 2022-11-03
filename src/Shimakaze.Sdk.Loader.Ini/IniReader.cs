using System.Text;

namespace Shimakaze.Sdk.Loader.Ini;

public sealed class IniReader : TokenReaderBase<IniToken>
{
    public IniReader(TextReader reader) : base(reader)
    {
    }

    public override string Value => _builder.ToString().Trim();

    private readonly StringBuilder _builder = new();

    public override bool Read()
    {
        _builder.Clear();

        ReadAllWhiteSpaceChar();

        int ch = _reader.Read();
        switch (ch)
        {
            case '#':
                Token = IniToken.PreProcessorCommand;
                return ReadEndOfLine();
            case ';':
                Token = IniToken.Comment;
                return ReadEndOfLine();
            case '[':
                Token = IniToken.SectionHeader;
                return ReadEndOfLine(']');
            case '=':
                Token = IniToken.Value;
                return ReadEndOfLine(';', true);
            default:
                Token = IniToken.Key;
                _builder.Append((char)ch);
                return ReadEndOfLine('=', true);
        }
    }

    /// <summary>
    /// End with EOL or Char
    /// </summary>
    /// <exception cref="FormatException">Not an EOL</exception>
    /// <returns>Success</returns>
    private bool ReadEndOfLine(int? endChar = null, bool skipEnd = false)
    {
        int ch;
        while ((ch = _reader.Peek()) is not -1)
        {
            if (ch is '\r' or '\n')
            {
                ch = _reader.Read();
                switch (ch)
                {
                    case '\n':
                        return true;
                    case '\r':
                        {
                            ch = _reader.Peek();
                            if (ch is '\n')
                                _reader.Read();

                            return true;
                        }
                    default:
                        throw new FormatException($"Unsupported EOF: 0x{ch:X8}{(char)ch}");
                }
            }

            if (ch == endChar)
            {
                if (!skipEnd)
                    _reader.Read();
                return true;
            }

            _reader.Read();
            _builder.Append((char)ch);
        }

        return _builder.Length > 0;
    }
}