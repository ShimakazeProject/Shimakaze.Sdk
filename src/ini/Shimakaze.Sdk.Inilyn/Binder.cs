using Shimakaze.Sdk.Inilyn.Models.Symbol;
using Shimakaze.Sdk.Inilyn.Models.Syntax;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// 符号绑定
/// </summary>
public static class Binder
{
    /// <summary>
    /// 默认的节名称
    /// </summary>
    public const string DefaultSectionName = "; Default";

    private static IEnumerable<KeySymbol> Binding(this SectionSyntaxNode sectionNode)
    {
        foreach (var syntax in sectionNode.SectionData.GetChildren().OfType<KeyValuePairSyntaxNode>())
        {
            var key = syntax.Key.Token.Value.ToString();
            if (syntax.Value is null)
            {
                yield return new(key, null);
                continue;
            }

            var value = syntax.Value.Token.Value.ToString();
            yield return new(key, value);
        }
    }

    /// <summary>
    /// 将键集合解析为字典形式，用于快速查找。
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="section"></param>
    /// <param name="data">要解析的键集合。</param>
    /// <returns>表示键名到键对象映射的只读字典。</returns>
    private static void InsertData(this EngineContext engine, SectionSymbol section, IEnumerable<KeySymbol> data)
    {
        foreach (var key in data)
        {
            if (key.Parent is not null)
            {
                // TODO: Diagnostic 此符号已被添加到 key.Parent.Name 中。
            }
            if (section.Keys.ContainsKey(key.Name))
            {
                // TODO: Diagnostic
            }

            key.Parent = section;
            section.Keys[key.Name] = key;
        }
    }

    /// <summary>
    /// 根据文档节点生成符号
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    public static IEnumerable<SectionSymbol> Binding(this EngineContext engine, DocumentSyntaxNode documentNode)
    {
        Dictionary<string, SectionSymbol> map = [];

        foreach (var syntax in documentNode.GetChildren().OfType<SectionSyntaxNode>())
        {
            var name = syntax.SectionHeader?.SectionName.Token.Value.ToString() ?? DefaultSectionName;
            string? documentComment = null;
            if (syntax.DocumentComment is not null)
                documentComment = string.Join("\r\n", syntax.DocumentComment.GetChildren().Select(i => i.Token.Value.ToString().Trim()));

            var inherit = syntax.SectionHeader?.InheritSectionName?.Token.Value.ToString();

            if (!map.TryGetValue(name, out var symbol))
                map[name] = symbol = new(name, inherit, documentComment);

            if (!string.IsNullOrWhiteSpace(symbol.Description) && !string.IsNullOrWhiteSpace(documentComment))
                symbol.Description = string.Join("\r\n", symbol.Description, documentComment);
            else if (!string.IsNullOrWhiteSpace(documentComment))
                symbol.Description ??= documentComment;

            engine.InsertData(symbol, syntax.Binding());
        }

        return map.Values;
    }
}
