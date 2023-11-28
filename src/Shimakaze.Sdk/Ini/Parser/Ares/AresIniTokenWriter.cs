namespace Shimakaze.Sdk.Ini.Parser.Ares;

/// <summary>
/// Ares INI 方言语法流写入器
/// </summary>
/// <param name="textWriter"></param>
/// <param name="leaveOpen"></param>
public class AresIniTokenWriter(TextWriter textWriter, bool leaveOpen = false) : IniTokenWriter(textWriter, leaveOpen)
{
    /// <inheritdoc/>
    public override void Write(in IniToken token)
    {
        switch (token.Type)
        {
            case AresIniTokenType.BASE_SECTION:
            case AresIniTokenType.PLUS:
                {
                    BaseWriter.Write((char)token.Type);
                    break;
                }
            default:
                base.Write(token);
                break;
        }
    }
}