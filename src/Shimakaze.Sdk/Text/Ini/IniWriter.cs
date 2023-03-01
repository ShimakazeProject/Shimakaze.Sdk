using Shimakaze.Sdk.Text.Ini.Serialization;

namespace Shimakaze.Sdk.Text.Ini;

/// <summary>
/// An INI Writer.
/// </summary>
public class IniWriter : ITokenWriter<int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IniWriter"/> class.
    /// </summary>
    /// <param name="writer">text writer.</param>
    public IniWriter(TextWriter writer)
    {
        this.Writer = writer;
        this.Options = IniSerializerOptions.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniWriter"/> class.
    /// </summary>
    /// <param name="writer">text writer.</param>
    /// <param name="options">options.</param>
    public IniWriter(TextWriter writer, IniSerializerOptions? options)
      : this(writer) => this.Options = options ??= IniSerializerOptions.Default;

    /// <summary>
    /// Gets text writer.
    /// </summary>
    protected TextWriter Writer { get; }

    /// <summary>
    /// Gets options.
    /// </summary>
    protected IniSerializerOptions Options { get; }

    /// <summary>
    /// Gets or sets a value indicating whether current is New Line.
    /// </summary>
    protected bool IsNewLine { get; set; } = true;

    /// <inheritdoc />
    public virtual void Write(int token, string value)
    {
        switch (token)
        {
            case IniToken.EmptyLine:
                this.Writer.Write(Environment.NewLine);
                this.IsNewLine = true;
                break;
            case IniToken.SectionHeader:
                this.Writer.Write('[');
                this.Writer.Write(value);
                this.Writer.Write(']');
                this.IsNewLine = false;
                break;
            case IniToken.Key:
                this.Writer.Write(value);
                this.IsNewLine = false;
                break;
            case IniToken.Value:
                if (!this.IsNewLine && this.Options.WrapSigns)
                {
                    this.Writer.Write(' ');
                }

                this.Writer.Write('=');

                if (this.Options.WrapSigns)
                {
                    this.Writer.Write(' ');
                }

                this.Writer.Write(value);
                this.IsNewLine = false;
                break;
            case IniToken.Comment:
                if (this.Options.IgnoreSummary)
                {
                    break;
                }

                if (!this.IsNewLine && this.Options.WrapSigns)
                {
                    this.Writer.Write(' ');
                }

                this.Writer.Write(';');

                if (this.Options.WrapSigns)
                {
                    this.Writer.Write(' ');
                }

                this.Writer.Write(value);
                this.IsNewLine = false;
                break;
            default:
                throw new NotSupportedException();
        }
    }
}
