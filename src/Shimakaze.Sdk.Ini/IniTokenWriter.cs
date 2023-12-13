namespace Shimakaze.Sdk.Ini;

/// <summary>
/// INI Token 流写入器
/// </summary>
/// <param name="textWriter"></param>
/// <param name="leaveOpen"></param>
public sealed class IniTokenWriter(TextWriter textWriter, bool leaveOpen = false)
    : BaseIniTokenWriter<IniDocument, IniSection>(textWriter, leaveOpen)
{
    /// <inheritdoc/>
    public override void Write(in IIniToken token)
    {
        switch (token.Token)
        {
            case 1:
                BaseWriter.Write(token.Value);
                break;
            case '\r':
            case '\n':
            case ' ':
            case '\t':
            case ';':
            case '=':
            case '[':
            case ']':
                BaseWriter.Write((char)token.Token);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 写INI文件
    /// </summary>
    /// <param name="document"></param>
    public override void Write(in IniDocument document)
    {
        Write(document.DefaultSection);
        foreach (var item in document)
            Write(item);
    }

    /// <summary>
    /// 写入节主体
    /// </summary>
    /// <param name="section"></param>
    public override void Write(in IniSection section)
    {
        if (section.Name != ";Default;")
        {
            Write(IniTokenTools.BeginBracket);
            Write(IniTokenTools.Value(section.Name));
            Write(IniTokenTools.EndBracket);
            Write(IniTokenTools.LF);
        }
        foreach (var item in section)
            Write(item);
    }

    /// <summary>
    /// 写入键值对
    /// </summary>
    /// <param name="keyValuePair"></param>
    public override void Write(in KeyValuePair<string, string> keyValuePair)
    {
        Write(IniTokenTools.Value(keyValuePair.Key));
        Write(IniTokenTools.EQ);
        Write(IniTokenTools.Value(keyValuePair.Value));
        Write(IniTokenTools.LF);
    }
}