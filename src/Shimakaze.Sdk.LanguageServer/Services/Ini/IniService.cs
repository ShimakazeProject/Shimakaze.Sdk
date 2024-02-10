using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Shimakaze.Sdk.LanguageServer.Models;
using Shimakaze.Sdk.LanguageServer.Services.Ini.Parser;

namespace Shimakaze.Sdk.LanguageServer.Services.Ini;
internal sealed class IniService(DataManager dataManager)
{
    public readonly SemanticTokensLegend Legend = new()
    {
        TokenTypes = new([
            SemanticTokenType.Class,
            SemanticTokenType.Property,
            SemanticTokenType.Operator,
            SemanticTokenType.String,
            SemanticTokenType.Number,
            SemanticTokenType.Keyword,
            SemanticTokenType.Comment,
        ])
    };

    public void Open(DocumentUri uri, string text, int? version)
    {
        dataManager.Context[uri] ??= new()
        {
            Attribute = new IniDocumentAttributes(uri, IniType.Unknown),
            Version = version,
        };

        // TODO: 后续需要改为从 msbuild 项目中读取是否支持ares语法
        IniParser parser = IniParser.CreateAres();
        using StringReader reader = new(text);
        List<IniSymbol> tokens = parser.Parse(reader);

        List<FoldingRange> foldingRanges = [];
        GetCommentFoldings(tokens, foldingRanges);
        GetSectionFoldings(tokens, foldingRanges);
    }

    public void Tokenize(SemanticTokensBuilder builder, DocumentUri uri)
    {
        var tokens = ((IniDocumentAttributes)dataManager.Context[uri].Attribute).Tokens;
        foreach (IniSymbol symbol in tokens)
            builder.Push(symbol.Line, symbol.StartCharacter, symbol.Length, IniTokenToSemanticTokenType(symbol));
    }

    private static SemanticTokenType? IniTokenToSemanticTokenType(in IniSymbol symbol) => symbol.Token switch
    {
        IniTokens.Comment => SemanticTokenType.Comment,
        IniTokens.BaseSection => SemanticTokenType.Class,
        IniTokens.Section => SemanticTokenType.Class,
        IniTokens.AddKey => SemanticTokenType.Operator,
        IniTokens.Key => SemanticTokenType.Property,
        IniTokens.Value => GetValueToken(symbol),
        _ => null
    };

    private static SemanticTokenType? GetValueToken(in IniSymbol symbol)
    {
        if (ParseNumber(symbol.Word))
            return SemanticTokenType.Number;
        else if (ParseBoolean(symbol.Word))
            return SemanticTokenType.Keyword;

        return SemanticTokenType.String;
    }

    private static bool ParseNumber(in ReadOnlySpan<char> str)
    {
        var word = str.Trim();
        if (word[^1] is '%')
            word = word[..^1];

        return decimal.TryParse(word, out _);
    }

    private static bool ParseBoolean(in ReadOnlySpan<char> str)
    {
        //return [0] is 'y' or 'Y' or 't' or 'T' or 'n' or 'N' or 'f' or 'F';
        var word = str.Trim();
        return word.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || word.Equals("no", StringComparison.OrdinalIgnoreCase)
            || word.Equals("true", StringComparison.OrdinalIgnoreCase)
            || word.Equals("false", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 找出当前文档中的所有的 多行注释 和它的范围
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="foldings"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private void GetCommentFoldings(List<IniSymbol> tokens, List<FoldingRange> foldings)
    {
        int commentId = Legend.GetTokenTypeIdentity((SemanticTokenType?)SemanticTokenType.Comment);

        IniSymbol? begin = null;
        IniSymbol? end = null;
        foreach (var current in tokens)
        {
            if (current.Token != commentId && begin is not null && end is not null)
            {
                if (begin != end)
                {
                    foldings.Add(new()
                    {
                        StartLine = begin.Line,
                        StartCharacter = begin.StartCharacter,
                        EndLine = end.Line,
                        EndCharacter = end.EndCharacter,
                        CollapsedText = begin.Word,
                        Kind = FoldingRangeKind.Comment
                    });
                }
                begin = null;
                end = null;
            }
            else if (current.Token == commentId)
            {
                begin ??= current;
                end = current;
            }
        }
        if (begin is not null && end is not null && begin != end)
        {
            foldings.Add(new()
            {
                StartLine = begin.Line,
                StartCharacter = begin.StartCharacter,
                EndLine = end.Line,
                EndCharacter = end.EndCharacter,
                CollapsedText = begin.Word,
                Kind = FoldingRangeKind.Comment
            });
        }
    }

    /// <summary>
    /// 找出当前文档中的所有的 section 和它的范围
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="foldings"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private void GetSectionFoldings(List<IniSymbol> tokens, List<FoldingRange> foldings)
    {
        int sectionId = Legend.GetTokenTypeIdentity((SemanticTokenType?)SemanticTokenType.Type);

        IniSymbol? section = null;
        IniSymbol? latest = null;
        foreach (var current in tokens)
        {
            if (current.Token == sectionId)
            {
                if (section is not null && latest is not null && section != latest)
                {
                    foldings.Add(new()
                    {
                        StartLine = section.Line,
                        StartCharacter = section.StartCharacter,
                        EndLine = latest.Line,
                        EndCharacter = latest.EndCharacter,
                        CollapsedText = section.Word,
                        Kind = FoldingRangeKind.Region,
                    });
                }
                section = current;
                latest = current;
            }
            else
            {
                latest = current;
            }
        }
        if (section is not null && latest is not null && section != latest)
        {
            foldings.Add(new()
            {
                StartLine = section.Line,
                StartCharacter = section.StartCharacter,
                EndLine = latest.Line,
                EndCharacter = latest.EndCharacter,
                CollapsedText = section.Word,
                Kind = FoldingRangeKind.Region,
            });
        }
    }


}
