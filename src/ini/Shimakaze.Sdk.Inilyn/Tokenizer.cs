using Shimakaze.Sdk.Inilyn.Models.Token;
using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// INI 词法分析器
/// </summary>
public static class Tokenizer
{
    /// <summary>
    /// 分析 INI 词法
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="text"></param>
    public static IEnumerable<IniToken> Tokenize(this EngineContext engine, SourceText text)
    {
        for (int lineNum = 0; lineNum < text.LineCount; lineNum++)
        {
            var lineText = text.GetLine(lineNum);
            if (lineText.IsWhiteSpace)
            {
                yield return IniTokenType.EOL.Create(lineText[^0..]);
                continue;
            }

            switch (lineText[0])
            {
                // 预处理器指令
                case '#':
                    yield return IniTokenType.Hash.Create(lineText[..1]);
                    yield return IniTokenType.Value.Create(lineText[1..].Trim());
                    yield return IniTokenType.EOL.Create(lineText[^0..]);
                    continue;
                // 文档注释
                case ';' when lineText.StartsWith(";;;"):
                    yield return IniTokenType.TripleSemicolon.Create(lineText[..3]);
                    yield return IniTokenType.Value.Create(lineText[3..].Trim());
                    yield return IniTokenType.EOL.Create(lineText[^0..]);
                    continue;
                // 注释
                case ';':
                    yield return IniTokenType.Semicolon.Create(lineText[..1]);
                    yield return IniTokenType.Value.Create(lineText[1..].Trim());
                    yield return IniTokenType.EOL.Create(lineText[^0..]);
                    continue;
                // 节
                case '[':
                    yield return IniTokenType.LeftBracket.Create(lineText[..1]);
                    lineText = lineText[1..].TrimStart();

                    int end = lineText.IndexOf(']');
                    if (end is -1)
                    {
                        engine.Report(Diagnostics.INI1001(lineText, lineText.GetRange(..)));
                        yield return IniTokenType.Unknown.Create(lineText);
                        continue;
                    }

                    yield return IniTokenType.Value.Create(lineText[..end].Trim());
                    yield return IniTokenType.RightBracket.Create(lineText[end..(end + 1)]);
                    lineText = lineText[(end + 1)..].TrimStart();

                    // 继承节
                    if (lineText.Length > 0 && lineText[0] is ':')
                    {
                        yield return IniTokenType.Colon.Create(lineText[..1]);
                        lineText = lineText[1..].Trim();

                        if (lineText.Length <= 0 || lineText[0] is not '[')
                        {
                            engine.Report(Diagnostics.INI1002(lineText, lineText.GetRange(..)));
                            yield return IniTokenType.Unknown.Create(lineText);
                            continue;
                        }

                        yield return IniTokenType.LeftBracket.Create(lineText[..1]);
                        lineText = lineText[1..].TrimStart();

                        end = lineText.IndexOf(']');
                        if (end is -1)
                        {
                            engine.Report(Diagnostics.INI1001(lineText, lineText.GetRange(..)));
                            yield return IniTokenType.Unknown.Create(lineText);
                            continue;
                        }

                        yield return IniTokenType.Value.Create(lineText[..end].Trim());
                        yield return IniTokenType.RightBracket.Create(lineText[end..(end + 1)]);
                        lineText = lineText[(end + 1)..].TrimStart();
                    }

                    if (lineText.Length > 0 && lineText[0] is ';')
                        goto case ';';

                    if (!lineText.IsWhiteSpace)
                    {
                        engine.Report(Diagnostics.INI1003(lineText, lineText.GetRange(..)));
                        yield return IniTokenType.Unknown.Create(lineText);
                        continue;
                    }

                    yield return IniTokenType.EOL.Create(lineText[^0..]);
                    continue;
                default:
                    // 按分号拆分内容
                    var semi = lineText.IndexOf(';');
                    var data = semi is -1 ? lineText : lineText[..semi];
                    var comment = semi is -1 ? null : lineText[semi..];

                    // 按等号拆分键值对
                    var eq = data.IndexOf('=');
                    if (eq is not -1)
                    {
                        yield return IniTokenType.Value.Create(data[..eq].Trim());
                        yield return IniTokenType.Eq.Create(data[eq..(eq + 1)]);
                        data = data[(eq + 1)..];
                        if (!data.IsWhiteSpace)
                            yield return IniTokenType.Value.Create(data.Trim());
                    }
                    else
                    {
                        engine.Report(Diagnostics.INI1003(data, data.GetRange(..)));
                        yield return IniTokenType.Unknown.Create(data.Trim());
                    }

                    if (comment is not null)
                    {
                        lineText = comment;
                        goto case ';';
                    }

                    yield return IniTokenType.EOL.Create(lineText[^0..]);
                    continue;
            }
        }
    }
}
