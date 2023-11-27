using Shimakaze.Sdk.Ini.Parser;

namespace Shimakaze.Sdk.Ini.Binder;

/// <summary>
/// IniDocument 绑定器
/// </summary>
/// <param name="tokenReader"></param>
public sealed class IniDocumentBinder(IniTokenReader tokenReader)
{
    private readonly IniTokenReader _tokenReader = tokenReader;

    /// <summary>
    /// 绑定
    /// </summary>
    /// <returns></returns>
    public IniDocument Bind(IniDocument? ini = default)
    {
        ini = [];
        IniSection current = ini.Default;
        string? key = default;
        foreach (var token in _tokenReader)
        {
            if (token.Value is null)
                continue;

            switch (token.Type)
            {
                case IniTokenType.Section:
                    if (!ini.TryGetSection(token.Value, out var section))
                        section = ini[token.Value] = new(token.Value);

                    current = section;
                    break;
                case IniTokenType.Key:
                    if (key is not null)
                        current[key] = string.Empty;
                    key = token.Value;
                    break;
                case IniTokenType.Value:
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
        return ini;
    }
}