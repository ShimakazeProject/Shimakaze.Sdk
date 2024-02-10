using System.Reflection;

using Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;
using Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders.Ares;

namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser;

/// <summary>
/// INI解析器
/// </summary>
public sealed class IniParser
{
    private readonly IEnumerable<IIniParseProvider> _iniParseProviders;

    private IniParser(IEnumerable<IIniParseProvider> iniParseProviders)
    {
        _iniParseProviders = iniParseProviders;
    }

    /// <summary>
    /// 创建 <see cref="IniParser"/> 实例
    /// </summary>
    /// <param name="iniParseProviders"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static IniParser Create(IEnumerable<IIniParseProvider> iniParseProviders)
    {
        Dictionary<int, IIniParseProvider> providers = [];
        foreach (var item in iniParseProviders
            .Select(i => new
            {
                Attribute = i.GetType().GetCustomAttribute<IniSymbolProviderAttribute>()
                    ?? throw new InvalidCastException("实现 IIniParseProvider 接口的类必须应用 IniParseProviderAttribute"),
                Provider = i
            })
            .OrderBy(i => i.Attribute.Weight))
        {
            providers[item.Attribute.Token] = item.Provider;
        }

        return new(providers.Values);
    }

    /// <summary>
    /// 创建默认 <see cref="IniParser"/> 实例
    /// </summary>
    /// <returns></returns>
    public static IniParser Create()
    {
        List<IIniParseProvider> providers =
        [
            new IniCommentTokenProvider(),
            new IniSectionTokenProvider(),
            new IniKeyTokenProvider(),
            new IniValueTokenProvider(),
        ];
        return Create(providers);
    }
    /// <summary>
    /// 创建支持 Ares INI 语法的 <see cref="IniParser"/> 实例
    /// </summary>
    /// <returns></returns>
    public static IniParser CreateAres()
    {

        List<IIniParseProvider> providers =
        [
            new IniCommentTokenProvider(),
            new IniSectionTokenProvider(),
            new IniKeyTokenProvider(),
            new IniValueTokenProvider(),
            new AresIniBaseSectionProvider(),
            new AresIniAddKeyProvider(),
        ];
        return Create(providers);
    }

    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="reader">读取器</param>
    /// <returns>解析出来的符号</returns>
    public List<IniSymbol> Parse(TextReader reader)
    {
        List<IniSymbol> symbols = [];

        List<(int Start, int End)> ranges = [];
        int lineNum = -1;
        while (reader.Peek() >= 0)
        {
            lineNum++;
            string? line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            ranges.Clear();
            var parsers = _iniParseProviders.Where(i => i.CanExecute(line));
            foreach (var parser in parsers)
            {
                IniSymbol token = parser.Execute(line, lineNum);
                symbols.Add(token);
                ranges.Add((token.StartCharacter, token.EndCharacter));
            }

            // 将未能处理的内容标记为Unknown
            int? start = null;
            for (int i = 0; i < line.Length; i++)
            {
                if (ranges.Any(j => j.Start <= i && i <= j.End) || start.HasValue && char.IsWhiteSpace(line[i]))
                {
                    if (start.HasValue)
                    {
                        int length = i - start.Value;
                        symbols.Add(IniSymbol.Unknown(lineNum, start.Value, length, line.AsSpan(start.Value, length)));
                        start = null;
                    }
                }
                else if (!start.HasValue)
                    start = i;
            }
        }

        return symbols;
    }
}
