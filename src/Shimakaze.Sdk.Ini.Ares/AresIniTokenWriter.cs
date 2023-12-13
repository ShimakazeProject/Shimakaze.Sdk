namespace Shimakaze.Sdk.Ini.Ares;

/// <summary>
/// INI Token 流写入器
/// </summary>
/// <param name="textWriter"></param>
/// <param name="leaveOpen"></param>
public sealed class AresIniTokenWriter(TextWriter textWriter, bool leaveOpen = false)
    : BaseIniTokenWriter<AresIniDocument, AresIniSection>(textWriter, leaveOpen)
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
            case ':':
            case '+':
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
    public override void Write(in AresIniDocument document)
    {
        Write(document.DefaultSection);
        foreach (var item in document)
            Write(item);
    }

    /// <summary>
    /// 写入节主体
    /// </summary>
    /// <param name="section"></param>
    public override void Write(in AresIniSection section)
    {
        if (section.Name != ";Default;")
        {
            Write(AresIniTokenTools.BeginBracket);
            Write(AresIniTokenTools.Value(section.Name));
            Write(AresIniTokenTools.EndBracket);
            if (string.IsNullOrEmpty(section.BaseName))
            {
                Write(AresIniTokenTools.COLON);
                Write(AresIniTokenTools.BeginBracket);
                Write(AresIniTokenTools.Value(section.Name));
                Write(AresIniTokenTools.EndBracket);
            }
            Write(AresIniTokenTools.LF);
        }
        foreach (var item in section.Raw)
            Write(item);
    }

    /// <summary>
    /// 写入键值对
    /// </summary>
    /// <param name="keyValuePair"></param>
    public override void Write(in KeyValuePair<string, string> keyValuePair)
    {
        Write(AresIniTokenTools.Value(keyValuePair.Key));
        Write(AresIniTokenTools.EQ);
        Write(AresIniTokenTools.Value(keyValuePair.Value));
        Write(AresIniTokenTools.LF);
    }
}