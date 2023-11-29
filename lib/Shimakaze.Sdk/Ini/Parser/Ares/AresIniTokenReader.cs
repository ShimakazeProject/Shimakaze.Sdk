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
    protected override bool FlushBuffer([NotNullWhen(true)] out IniToken? result, int type = 0)
    {
        result = default;
        if (type is IniTokenType.Unknown
            && _buffer.Length is not 0
            && _buffer[0] is ':')
        {
            // 继承节
            result = new(AresIniTokenType.BASE_SECTION);
            _buffer.Clear();
        }
        else if (type is IniTokenType.Key
            && _buffer.Length is not 0
            && _buffer[0] is '+')
        {
            // += 注册表
            result = new(AresIniTokenType.PLUS);
            _buffer.Clear();
        }

        return result is not null || base.FlushBuffer(out result, type);
    }
}
