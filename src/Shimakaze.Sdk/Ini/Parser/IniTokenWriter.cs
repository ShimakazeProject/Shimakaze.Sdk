namespace Shimakaze.Sdk.Ini.Parser;

/// <summary>
/// INI Token 流写入器
/// </summary>
/// <param name="textWriter"></param>
/// <param name="leaveOpen"></param>
public class IniTokenWriter(TextWriter textWriter, bool leaveOpen = false) : IDisposable
{
    private bool _disposedValue;
    /// <summary>
    /// leave Open
    /// </summary>
    protected bool _leaveOpen = leaveOpen;

    /// <summary>
    /// 基础流
    /// </summary>
    public TextWriter BaseWriter { get; } = textWriter;

    /// <summary>
    /// 写入Token
    /// </summary>
    /// <param name="token"></param>
    public virtual void Write(in IniToken token)
    {
        switch (token.Type)
        {
            case IniTokenType.Comment:
                {
                    BaseWriter.Write(';');
                    BaseWriter.Write(token.Value);
                    break;
                }
            case IniTokenType.Section:
                {
                    BaseWriter.Write('[');
                    BaseWriter.Write(token.Value);
                    BaseWriter.Write(']');
                    break;
                }
            case IniTokenType.Key:
                {
                    BaseWriter.Write(token.Value);
                    break;
                }
            case IniTokenType.Value:
                {
                    BaseWriter.Write('=');
                    BaseWriter.Write(token.Value);
                    break;
                }
            case IniTokenType.CR:
            case IniTokenType.LF:
            case IniTokenType.SPACE:
            case IniTokenType.TAB:
                {
                    BaseWriter.Write((char)token.Type);
                    break;
                }
            default:
                throw new NotSupportedException();
        }
    }

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
                BaseWriter.Dispose();
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