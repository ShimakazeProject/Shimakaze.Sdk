using Shimakaze.Sdk.Inilyn.Models.Syntax;
using Shimakaze.Sdk.Inilyn.Models.Syntax.Green;
using Shimakaze.Sdk.Inilyn.Models.Token;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// INI 语法分析器
/// </summary>
public static class Parser
{
    private static IEnumerable<Range> BuildRanges(this IEnumerable<int> indexes)
    {
        int latest = 0;
        foreach (var item in indexes)
        {
            yield return latest..item;
            latest = item;
        }
    }

    private static void ParseCompilerCommand(this IniToken token, out string command, out string[] arguments)
    {
        var str = token.Value.AsSpan().Trim();
        List<int> whiteSpaceIndexes = [];
        Stack<char> stack = [];
        for (int i = 0; i < str.Length; i++)
        {
            var current = str[i];
            switch (current)
            {
                case '>' when stack.TryPeek(out var ch) && ch is '<':
                    stack.Pop();
                    continue;
                case '<':
                    stack.Push(current);
                    continue;
                case ']' when stack.TryPeek(out var ch) && ch is '[':
                    stack.Pop();
                    continue;
                case '[':
                    stack.Push(current);
                    continue;
                case '"' when stack.TryPeek(out var ch) && ch is '"':
                    stack.Pop();
                    continue;
                case '"':
                    stack.Push(current);
                    continue;
                case '\'' when stack.TryPeek(out var ch) && ch is '\'':
                    stack.Pop();
                    continue;
                case '\'':
                    stack.Push(current);
                    continue;
                default:
                    if (char.IsWhiteSpace(current) && stack.Count is 0)
                        whiteSpaceIndexes.Add(i);

                    continue;
            }
        }
        whiteSpaceIndexes.Add(str.Length);

        List<string> arr = [];
        foreach (var range in whiteSpaceIndexes.BuildRanges())
        {
            var span = str[range].Trim();
            if (!span.IsEmpty)
                arr.Add(span.ToString());
        }

        command = arr[0];
        arguments = [.. arr.Skip(1)];
    }

    private static bool Match(this IEnumerable<IniToken> tokens, params IEnumerable<IniTokenType> types)
    {
        using var tokensEnumerator = tokens.GetEnumerator();
        using var typesEnumerator = types.GetEnumerator();

        while (tokensEnumerator.MoveNext() && typesEnumerator.MoveNext())
        {
            if (tokensEnumerator.Current.Type != typesEnumerator.Current)
                return false;
        }

        return !typesEnumerator.MoveNext();
    }

    private static IEnumerable<IReadOnlyList<IniToken>> SplitByEOL(this IEnumerable<IniToken> tokens)
    {
        List<IniToken> tmp = [];
        foreach (var item in tokens)
        {
            if (item.Type is IniTokenType.EOL)
            {
                yield return tmp.AsReadOnly();
                tmp.Clear();
            }
            else
            {
                tmp.Add(item);
            }
        }

        if (tmp.Count is not 0)
            yield return tmp.AsReadOnly();
    }

    private static IEnumerable<GreenNode> ParseLine(this IEnumerable<IReadOnlyList<IniToken>> tokens, ParserContext context)
    {
        foreach (var line in tokens)
        {
            if (line.Count is 0)
                continue;

            int skip = 0;
            if (line.Match(IniTokenType.Hash, IniTokenType.Value))
            {
                line[1].ParseCompilerCommand(out var command, out var arguments);

                if (context.CompilerCommands.TryGetValue(command, out var method))
                    method.DynamicInvoke([context, .. arguments]);
            }
            else if (context.CanWritable && line.Match(IniTokenType.Semicolon, IniTokenType.Value))
            {
                yield return new CommentNode(line[1]);
            }
            else if (context.CanWritable && line.Match(IniTokenType.TripleSemicolon, IniTokenType.Value))
            {
                yield return new DocumentCommentNode(line[1]);
            }
            else if (context.CanWritable && line.Match(IniTokenType.LeftBracket, IniTokenType.Value, IniTokenType.RightBracket))
            {
                SectionNameNode sectionName = new(line[skip + 1]);
                InheritSectionNameNode? inheritSectionName = null;
                CommentNode? comment = null;
                skip += 3;

                if (line.Skip(skip).Match(IniTokenType.Colon, IniTokenType.LeftBracket, IniTokenType.Value, IniTokenType.RightBracket))
                {
                    inheritSectionName = new(line[skip + 2]);
                    skip += 4;
                }

                if (line.Skip(skip).Match(IniTokenType.Semicolon, IniTokenType.Value))
                {
                    comment = new(line[skip + 1]);
                    skip += 2;
                }

                yield return new SectionHeaderNode(sectionName, inheritSectionName, comment);
            }
            else if (context.CanWritable && line.Match(IniTokenType.Value, IniTokenType.Eq))
            {
                KeyNode key = new(line[skip]);
                ValueNode? value = null;
                CommentNode? comment = null;

                skip += 2;
                if (line.Skip(skip).Match(IniTokenType.Value))
                {
                    value = new(line[skip]);
                    skip += 1;
                }

                if (line.Skip(skip).Match(IniTokenType.Semicolon, IniTokenType.Value))
                {
                    comment = new(line[skip + 1]);
                    skip += 2;
                }

                yield return new KeyValuePairNode(key, value, comment);
            }
            else if (context.CanWritable)
            {
                // TODO: 不合法内容 应该创建一个错误节点
                // TODO: 即使是错误内容 也要判断后面有没有注释
                for (int i = 0; i < line.Count; i++)
                {
                    if (line[i].Type is not IniTokenType.Semicolon)
                        continue;

                    if (line.Skip(skip).Match(IniTokenType.Semicolon, IniTokenType.Value))
                    {
                        yield return new CommentNode(line[skip + 1]);
                    }
                }
            }
        }
    }

    private static IEnumerable<SectionNode> ParseSections(this IEnumerable<GreenNode> greens)
    {
        SectionHeaderNode? sectionHeader = null;
        List<GreenNode> data = [];
        List<DocumentCommentNode> documentComments = [];
        GreenNode? latest = null;
        foreach (var green in greens)
        {
            try
            {
                switch (green)
                {
                    case DocumentCommentNode doc:
                        documentComments.Add(doc);
                        continue;
                    case SectionHeaderNode header:
                        yield return new(new(data), sectionHeader, new(documentComments));
                        data = [];
                        documentComments = [];
                        sectionHeader = header;
                        continue;
                    default:
                        // 如果 sectionHeader 为空 则说明还没有进入任何一个节
                        // 此时这个文档注释应该作用于整个文件
                        // 虽然想法很美好 但它最终应该会被应用到第一个节上
                        // 后续应该想办法区分出哪些文档注释应该被应用到文件中
                        // 比如 被空白行隔断 上半部分是文件的文档注释 下半部分是节的
                        if (latest is DocumentCommentNode && sectionHeader is not null)
                        {
                            // 因为这里的文档注释并不合法 所以这里会当作普通注释处理
                            data.AddRange(documentComments);
                            documentComments.Clear();
                        }
                        data.Add(green);
                        continue;
                }
            }
            finally
            {
                latest = green;
            }
        }

        yield return new(new(data), sectionHeader, new(documentComments));
    }

    /// <summary>
    /// 分析 INI 语法
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public static DocumentSyntaxNode? Parse(this EngineContext engine, IEnumerable<IniToken> tokens)
    {
        if (!tokens.Any())
            return null;

        ParserContext context = new();
        return new(new(tokens.SplitByEOL().ParseLine(context).ParseSections()));
    }
}
