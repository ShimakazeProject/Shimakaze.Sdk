using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini.Parser.Ares;

/// <summary>
/// Ares INI 方言语法分析器
/// </summary>
/// <param name="textReader"></param>
/// <param name="ignore"></param>
/// <param name="leaveOpen"></param>
public class AresIniTokenReader(TextReader textReader, IniTokenIgnoreLevel ignore = IniTokenIgnoreLevel.NonValue, bool leaveOpen = false) : IniTokenReader(textReader, ignore, leaveOpen)
{
    /// <inheritdoc/>
    protected override bool FlushBuffer([NotNullWhen(true)] out IniToken? result, int type = IniTokenType.Unknown)
    {
        result = default;
        if (Buffer.Length is not 0)
        {
            switch ((type, Buffer[0]))
            {
                case (IniTokenType.Unknown, ':'):
                    // 继承节
                    result = new(AresIniTokenType.BASE_SECTION);
                    Buffer.Clear();
                    break;
                case (IniTokenType.Key, '+'):
                    // += 注册表
                    result = new(AresIniTokenType.PLUS);
                    Buffer.Clear();
                    break;
                default:
                    break;
            }
        }

        return result is not null || base.FlushBuffer(out result, type);
    }
}
