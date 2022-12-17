using System.Text;

namespace Shimakaze.Sdk.Text.Ini;

/// <summary>
/// An INI Reader.
/// </summary>
public class IniReader : ITokenReader<int>
{
    private readonly StringBuilder builder = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IniReader"/> class.
    /// </summary>
    /// <param name="reader">Text reader.</param>
    public IniReader(TextReader reader) => this.Reader = reader;

    /// <inheritdoc />
    public virtual int? Token { get; protected set; }

    /// <inheritdoc />
    public virtual string Value => this.builder.ToString().Trim();

    /// <summary>
    /// Gets text reader.
    /// </summary>
    protected TextReader Reader { get; }

    /// <inheritdoc />
    public virtual bool Read()
    {
        this.builder.Clear();

        int ch = this.Reader.Read();
        switch (ch)
        {
            case '#':
                this.Token = IniToken.PreProcessorCommand;
                return this.ReadEndOfLine();
            case ';':
                this.Token = IniToken.Comment;
                return this.ReadEndOfLine();
            case '[':
                this.Token = IniToken.SectionHeader;
                return this.ReadEndOfLine(']');
            case '=':
                this.Token = IniToken.Value;
                return this.ReadEndOfLine(';', true);
            case -1:
                return false;
            case '\r':
            case '\n':
                this.Token = IniToken.EmptyLine;
                return this.ReadAllWhiteSpaceChar();
            default:
                this.Token = IniToken.Key;
                this.builder.Append((char)ch);
                return this.ReadEndOfLine('=', true);
        }
    }

    /// <summary>
    /// End with EOL or Char.
    /// </summary>
    /// <param name="endChar">end char.</param>
    /// <param name="skipEnd">skip end char.</param>
    /// <exception cref="FormatException">Not an EOL.</exception>
    /// <returns>Success.</returns>
    protected virtual bool ReadEndOfLine(int? endChar = null, bool skipEnd = false)
    {
        int ch;
        while ((ch = this.Reader.Peek()) is not -1)
        {
            if (ch is '\r' or '\n')
            {
                ch = this.Reader.Read();
                switch (ch)
                {
                    case '\n':
                        return true;
                    case '\r':
                        {
                            ch = this.Reader.Peek();
                            if (ch is '\n')
                            {
                                this.Reader.Read();
                            }

                            return true;
                        }

                    default:
                        throw new FormatException($"Unsupported EOF: 0x{ch:X8}{(char)ch}");
                }
            }

            if (ch == endChar)
            {
                if (!skipEnd)
                {
                    this.Reader.Read();
                }

                return true;
            }

            this.Reader.Read();
            this.builder.Append((char)ch);
        }

        return this.builder.Length > 0;
    }

    /// <summary>
    /// Read all of white chars.
    /// </summary>
    /// <returns>Success.</returns>
    protected virtual bool ReadAllWhiteSpaceChar()
    {
        while (this.Reader.Peek() is '\x20' or '\t' or '\r' or '\n')
        {
            this.builder.Append((char)this.Reader.Read());
        }

        return true;
    }
}
