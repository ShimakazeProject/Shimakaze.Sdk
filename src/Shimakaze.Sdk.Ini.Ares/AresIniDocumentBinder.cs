using System.Text;

namespace Shimakaze.Sdk.Ini.Ares;

/// <summary>
/// IniDocument 绑定器
/// </summary>
/// <param name="tokenReader"></param>
/// <param name="options"></param>
/// <param name="leaveOpen"></param>
public sealed class AresIniDocumentBinder(
    AresIniTokenReader tokenReader,
    AresIniDocumentBinderOptions? options = default,
    bool leaveOpen = false
    )
    : IDisposable, IIniDocumentBinder<AresIniDocument, AresIniSection>
{
    private readonly bool _leaveOpen = leaveOpen;
    private readonly AresIniTokenReader _tokenReader = tokenReader;

    /// <inheritdoc/>
    public AresIniDocument Bind(AresIniDocument? ini = default)
    {
        options ??= AresIniDocumentBinderOptions.Default;
        ini ??= new(options.SectionComparer, options.KeyComparer);
        AresIniSection current = ini.DefaultSection;
        // 暂存区
        StringBuilder sb = new();

        // 状态
        bool isSection = false;
        bool isBaseSection = false;
        bool isComment = false;
        string? key = default;

        foreach (var token in _tokenReader)
        {
            switch (token.Token)
            {
                case ' ' when !isComment:
                    sb.Append(' ');
                    break;
                case '\t' when !isComment:
                    sb.Append('\t');
                    break;
                case 1 when !isComment && !string.IsNullOrEmpty(token.Value):
                    sb.Append(token.Value);
                    break;

                case '=' when !isComment && !isSection:
                    key = GetString(options.Trim);
                    if (key.Trim() == "+")
                    {
                        do
                        {
                            key = Guid.NewGuid().ToString();
                        } while (current.ContainsKey(key));
                    }
                    break;

                case '+' when !isComment && !isSection:
                    sb.Append('+');
                    break;

                case ':' when !isComment && !isSection && key is null:
                    Flush(options.Trim);
                    isBaseSection = true;
                    break;

                case ':' when !isComment && key is not null:
                    sb.Append(':');
                    break;

                case ';':
                    Flush(options.Trim);

                    // 重置状态
                    isComment = true;
                    isSection = false;
                    isBaseSection = false;
                    key = null;

                    break;
                case '\r':
                case '\n':
                    Flush(options.Trim);

                    // 重置状态
                    isComment = false;
                    isSection = false;
                    isBaseSection = false;
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
                    if (!isBaseSection)
                    {
                        if (!ini.TryGetSection(sectionName, out var section))
                            section = ini[sectionName] = new(sectionName, default, new(options.KeyComparer));
                        current = section;
                    }
                    else
                    {
                        current.BaseName = sectionName;
                    }
                    break;
            }
        }
        Flush(options.Trim);

        // 组织继承结构

        return BindDependencyTree(ini);

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

    /// <summary>
    /// 绑定或重新绑定继承树
    /// </summary>
    /// <param name="ini"></param>
    /// <returns></returns>
    public AresIniDocument BindDependencyTree(AresIniDocument? ini = default)
    {
        options ??= AresIniDocumentBinderOptions.Default;
        ini ??= new(options.SectionComparer, options.KeyComparer);
        // 组织继承结构
        foreach (var item in ini)
        {
            if (string.IsNullOrEmpty(item.BaseName))
                continue;

            if (ini.TryGetSection(item.BaseName, out var section))
                item.Base = section;
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
