using Shimakaze.Sdk.Ini.Ares;
using Shimakaze.Sdk.Ini.Parser;
using Shimakaze.Sdk.Ini.Parser.Ares;

namespace Shimakaze.Sdk.Ini.Binder.Ares;

/// <summary>
/// AresIniDocument 绑定器
/// </summary>
/// <param name="tokenReader"></param>
/// <param name="leaveOpen"></param>
public sealed class AresIniDocumentBinder(AresIniTokenReader tokenReader, bool leaveOpen = false) : IDisposable
{
    private readonly bool _leaveOpen = leaveOpen;
    private readonly AresIniTokenReader _tokenReader = tokenReader;

    /// <summary>
    /// 绑定
    /// </summary>
    /// <returns></returns>
    public AresIniDocument Bind(AresIniDocument? ini = default)
    {
        ini = [];
        AresIniSection current = ini.Default;
        string? key = default;
        bool isBase = false;
        foreach (var token in _tokenReader)
        {
            switch (token.Type)
            {
                // 继承节符号
                case AresIniTokenType.BASE_SECTION:
                    isBase = true;
                    break;
                // 继承节
                case IniTokenType.Section when isBase:
                    current.BaseName = token.Value;
                    isBase = false;
                    break;
                // 节
                case IniTokenType.Section when !isBase && !string.IsNullOrEmpty(token.Value):
                    if (!ini.TryGetSection(token.Value, out var section))
                        section = ini[token.Value] = new(token.Value);

                    current = section;
                    break;
                // 随机键名
                case AresIniTokenType.PLUS:
                    if (key is not null)
                        current[key] = string.Empty;
                    key = Guid.NewGuid().ToString();
                    break;
                // 键
                case IniTokenType.Key:
                    if (key is not null)
                        current[key] = string.Empty;
                    key = token.Value;
                    break;
                // 值
                case IniTokenType.Value when !string.IsNullOrEmpty(token.Value):
                    if (key is null)
                    {
                        current[token.Value] = string.Empty;
                    }
                    else
                    {
                        current[key] = token.Value;
                        key = default;
                    }
                    break;
            }
        }

        // 组织继承节
        foreach (var section in ini)
        {
            if (!string.IsNullOrEmpty(section.BaseName) && ini.TryGetSection(section.BaseName, out var baseSection))
                section.Base = baseSection;
        }
        return ini;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_leaveOpen)
            _tokenReader.Dispose();
    }
}