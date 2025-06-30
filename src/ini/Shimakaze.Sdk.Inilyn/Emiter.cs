using Shimakaze.Sdk.Inilyn.Models.Emit;
using Shimakaze.Sdk.Inilyn.Models.Symbol;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// 创建Ini文档
/// </summary>
public static class Emitter
{
    /// <summary>
    /// 创建 Ini 文档
    /// </summary>
    /// <param name="context"></param>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static IniDocument Emit(this EngineContext context, IEnumerable<SectionSymbol> symbols)
    {
        Dictionary<string, IniSection> doc = [];
        foreach (var symbol in symbols)
        {
            doc[symbol.Name] = new(symbol.Name, symbol.Data
                .Where(i => i.Value is not null)
                .Select(i => KeyValuePair.Create(i.Key, i.Value!.RawText)));
        }
        return new(doc);
    }

    /// <summary>
    /// 合并多个 ini 文档
    /// </summary>
    /// <param name="context"></param>
    /// <param name="documents"></param>
    /// <returns></returns>
    public static IniDocument Merge(this EngineContext context, params IEnumerable<IniDocument> documents)
    {
        Dictionary<string, Dictionary<string, string>> squash = [];
        foreach (var section in documents.SelectMany(i => i.Sections))
        {
            if (!squash.TryGetValue(section.Name, out var data))
                squash[section.Name] = data = [];

            foreach (var kvp in section)
                data[kvp.Key] = kvp.Value;
        }

        return new(squash.Select(i => KeyValuePair.Create<string, IniSection>(i.Key, new(i.Key, i.Value))));
    }
}
