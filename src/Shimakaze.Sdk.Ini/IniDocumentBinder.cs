using System.Text;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// IniDocument 绑定器
/// </summary>
/// <param name="tokenReader"></param>
/// <param name="options"></param>
/// <param name="leaveOpen"></param>
public sealed class IniDocumentBinder(
    IniTokenReader tokenReader,
    IniDocumentBinderOptions? options = default,
    bool leaveOpen = false
    )
    : IDisposable, IIniDocumentBinder<IniDocument, IniSection>
{
    private readonly bool _leaveOpen = leaveOpen;
    private readonly IniTokenReader _tokenReader = tokenReader;

    /// <inheritdoc/>
    public IniDocument Bind(IniDocument? ini = default)
    {
        options ??= IniDocumentBinderOptions.Default;
        ini ??= new(options.SectionComparer, options.KeyComparer);
        IniSection current = ini.DefaultSection;
        // 暂存区
        StringBuilder sb = new();

        // 状态
        bool isSection = false;
        bool isComment = false;
        string? key = default;

        foreach (var token in _tokenReader)
        {
            switch (token.Token)
            {
                case ' ' when !isComment:
                    sb.Append(' ').Append(token.Value);
                    break;
                case '\t' when !isComment:
                    sb.Append('\t').Append(token.Value);
                    break;
                case 1 when !isComment && !string.IsNullOrEmpty(token.Value):
                    sb.Append(token.Value);
                    break;

                case '=' when !isSection:
                    key = GetString(options.Trim);
                    break;

                case ';':
                    Flush(options.Trim);

                    // 重置状态
                    isComment = true;
                    isSection = false;
                    key = null;

                    break;
                case '\r':
                case '\n':
                    Flush(options.Trim);

                    // 重置状态
                    isComment = false;
                    isSection = false;
                    key = null;

                    break;

                // 读Section
                case '[' when !isComment:
                    Flush(options.Trim);
                    isSection = true;
                    break;
                case ']' when !isComment:
                    isSection = false;
                    string sectionName = GetString(false);
                    if (!ini.TryGetSection(sectionName, out var section))
                        section = ini[sectionName] = new(sectionName, new(options.KeyComparer));
                    current = section;
                    break;
            }
        }
        Flush(options.Trim);

        return ini;

        string GetString(bool trim)
        {
            string result = sb.ToString();
            sb.Clear();
            return trim ? result.Trim() : result;
        }
        void Flush(bool trim)
        {
            string value = GetString(trim);
            if (!string.IsNullOrEmpty(value))
            {
                if (isSection)
                    value = '[' + value;
                if (!string.IsNullOrEmpty(key))
                    current[key] = value;
                else
                    current[value] = string.Empty;
            }
            else if (!string.IsNullOrEmpty(key))
            {
                current[key] = string.Empty;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_leaveOpen)
            _tokenReader.Dispose();
    }
}
