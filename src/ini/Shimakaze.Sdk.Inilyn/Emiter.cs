using Shimakaze.Sdk.Inilyn.Models.Emit;
using Shimakaze.Sdk.Inilyn.Models.Symbol;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// 创建Ini文档
/// </summary>
public static class Emitter
{
    /// <summary>
    /// 创建Ini文档
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
}
