using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Ini.Directives;
using Shimakaze.Sdk.Ra2lyn.Ini.Lexing;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// INI 语法分析器：把记号序列组合为语法树。
/// </summary>
/// <remarks>
/// <para>
/// 语法（BNF）：
/// </para>
/// <code>
/// ini-document         ::= trivia* entry* trivia* eof
/// entry                ::= section-declaration | key-value-entry
/// section-declaration  ::= trivia* '[' section-name ']' ( ':' inheritance-list )? trailing-trivia child-entry*
/// child-entry          ::= key-value-entry                    （直到下一个节头或 EOF）
/// inheritance-list     ::= ws? inheritance ( ws? ',' ws? inheritance )*
/// inheritance          ::= '[' section-name ']'
/// key-value-entry      ::= trivia* key ws? '=' value-trailing-trivia
/// value-trailing-trivia::= 行内到行尾的所有记号（按原文拼接）直至（不含）换行；换行进入尾随 trivia
/// section-name         ::= (Text | Whitespace)*               （允许含空白，首尾裁剪）
/// </code>
/// <para>
/// 只有真正的语法问题才产生诊断：缺少 <c>]</c>、缺少节名、缺少继承分隔符、缺少 <c>=</c>、缺少键、
/// 缺少值、键含空白、无法识别的记号。重复节与重复键属于语义问题，由降级阶段报告。
/// </para>
/// </remarks>
public static class IniParser
{
    /// <summary>
    /// 对源文件进行语法分析。
    /// </summary>
    /// <param name="source">待分析的源文件。</param>
    /// <returns>语法树，其中包含本次分析产生的全部语法诊断。</returns>
    public static IniSyntaxTree Parse(SourceFile source)
        => Parse(source, [], null, []);

    /// <summary>
    /// 对源文件进行语法分析，并附带预处理阶段产生的诊断与已预处理的记号流。
    /// </summary>
    /// <param name="source">待分析的源文件。</param>
    /// <param name="preprocessorDiagnostics">预处理诊断，会与语法诊断合并输出。</param>
    /// <param name="tokens">已预处理的记号流；为 <see langword="null"/> 时直接对源文本做词法分析。</param>
    /// <param name="templateModes">预处理阶段收集到的 <c>#pragma template on|off</c> 指令，按出现顺序排列。</param>
    public static IniSyntaxTree Parse(
        SourceFile source,
        IReadOnlyList<Diagnostic> preprocessorDiagnostics,
        IReadOnlyList<IniToken>? tokens = null,
        IReadOnlyList<TemplateModeDirective>? templateModes = null)
        => new ParserState(source, preprocessorDiagnostics, tokens, templateModes).Parse();

    /// <summary>
    /// 语法分析的可变状态；每次 <see cref="IniParser.Parse(SourceFile)"/> 使用独立实例，因此可重入。
    /// </summary>
    private sealed class ParserState
    {
        private readonly string _path;
        private readonly int _length;
        private readonly IReadOnlyList<IniToken> _tokens;
        private readonly IReadOnlyList<TemplateModeDirective> _templateModes;
        private readonly List<Diagnostic> _diagnostics = [];
        private int _position;

        public ParserState(
            SourceFile source,
            IReadOnlyList<Diagnostic> preprocessorDiagnostics,
            IReadOnlyList<IniToken>? tokens,
            IReadOnlyList<TemplateModeDirective>? templateModes)
        {
            _path = source.Path;
            _length = source.Text.Length;
            _tokens = tokens ?? IniLexer.Tokenize(source.Text, source.Path);
            _templateModes = templateModes ?? [];

            // 预处理诊断先进入诊断表，保证输出顺序稳定。
            foreach (var diagnostic in preprocessorDiagnostics)
                _diagnostics.Add(diagnostic);
        }

        private IniToken Current
            => _position < _tokens.Count ? _tokens[_position] : _tokens[^1];

        public IniSyntaxTree Parse()
        {
            // 文件头 trivia 只包含第一个条目之前的注释与空白；条目之间与文件尾的空行分别属于
            // 下一条目的前导 trivia 与文件尾 trivia，不能整体并入文件头。
            List<SyntaxNode> entries = [];
            List<Trivia> leading = CollectTrivia();
            List<Trivia> fileHead = leading;

            while (Current.Type is not IniTokenType.EndOfFile)
            {
                if (Current.Type is IniTokenType.LeftBracket)
                {
                    entries.Add(ParseSection(leading));
                }
                else if (Current.IsKeyStart)
                {
                    KeyValueNode? keyValue = ParseKeyValue(leading);
                    if (keyValue is not null)
                        entries.Add(keyValue);
                }
                else if (Current.Type is IniTokenType.EqualSign)
                {
                    // 行首就是 '='：本行没有键名，报告 INI007 并跳过该行。
                    Report(IniDiagnostics.MissingKey(Current.Span));
                    SkipLine();
                }
                else if (Current.Type is IniTokenType.RightBracket && entries.Count is not 0)
                {
                    // 未闭合的键值行以 ']' 结束：']' 不属于任何子节点，跳过。
                    Advance();
                }
                else
                {
                    Report(IniDiagnostics.UnexpectedToken(Current.Text, Current.Span));
                    Advance();
                }

                if (Current.Type is IniTokenType.EndOfFile)
                    break;

                leading = CollectTrivia();
                if (entries.Count is 0)
                {
                    // 第一个条目之前的全部 trivia 都是文件头。
                    fileHead = leading;
                    leading = [];
                }
            }

            IniDocumentNode root = new()
            {
                LeadingTrivia = fileHead,
                Entries = entries,
                TrailingTrivia = leading,
                Span = _length is 0 ? default : new(_path, 0, _length),
            };

            return new IniSyntaxTree { Root = root, Diagnostics = _diagnostics };
        }

        private SyntaxNode ParseSection(List<Trivia> leading)
        {
            IniToken open = Advance();
            NamedText name = ReadName();

            IniToken? close = null;
            if (TryMatch(IniTokenType.RightBracket, out IniToken closeToken))
                close = closeToken;
            else
                Report(IniDiagnostics.MissingRightBracket(Current.Span));

            if (name.Text.Length is 0)
                Report(IniDiagnostics.MissingName(name.Span));

            // 继承子句：冒号前允许空白；冒号本身不是语法节点，空白直接跳过。
            IniToken? colon = null;
            if (IsFollowedByToken(IniTokenType.Colon))
            {
                while (Current.Type is IniTokenType.Whitespace)
                    Advance();

                colon = Advance();
            }

            // 判断冒号后面跟的是 '[', 'where'，还是别的什么。
            bool colonFollowedByInheritance = IsFollowedByToken(IniTokenType.LeftBracket);
            bool colonFollowedByWhere = TryPeekWhereKeyword();

            bool nameStartsWithDollar = name.Text.Length > 0 && name.Text[0] is '$';

            // 决策表：
            //   name 以 '$' 开头 + ':' 后跟 'where'  → TemplateNode（带条件）
            //   name 以 '$' 开头 + 无 ':' + TemplateMode=on → TemplateNode（无条件）
            //   name 以 '$' 开头 + 无 ':' + TemplateMode=off → SectionNode（保留 '$'）
            //   name 以 '$' 开头 + ':' 后跟 '[' → SectionNode（继承列表）
            //   name 不以 '$' 开头 + ':' 后跟 'where' → TemplateNode（兼容宽松处理）
            //   其他 → 走原 SectionNode / 继承列表逻辑。
            bool wantsTemplate =
                (colon is not null && colonFollowedByWhere)
                || (colon is null && nameStartsWithDollar && IsTemplateModeOn(open.Span.Start));

            if (wantsTemplate)
            {
                return ParseTemplateSection(open, close, name, colon, leading,
                    colonFollowedByWhere, colonFollowedByInheritance);
            }

            if (colon is not null && nameStartsWithDollar && colonFollowedByInheritance is false && colonFollowedByWhere is false)
            {
                // $ 开头节写了 ':'，但既不是 '[' 也不是 'where'：模板节语法错误。
                Report(IniDiagnostics.TemplateWhereExpected(Current.Span));
            }

            List<SectionInheritance> inheritances = ParseInheritanceList(colon);

            // 节头行的剩余部分（行内注释与换行）是节节点的尾随 trivia。
            List<Trivia> trailing = CollectRestOfLine();

            List<KeyValueNode> keyValues = ParseBody();

            SourceSpan header = MakeHeaderSpan(open.Span.Start, close, inheritances);
            int end = header.IsEmpty
                ? name.Span.End
                : header.End;

            return new SectionNode
            {
                Name = name.Text,
                HasOpenBracket = true,
                HasCloseBracket = close is not null,
                Inheritances = inheritances,
                KeyValues = keyValues,
                LeadingTrivia = leading,
                TrailingTrivia = trailing,
                NameSpan = name.Span,
                HeaderSpan = header,
                Span = new(_path, open.Span.Start, end),
            };
        }

        /// <summary>
        /// 解析模板节 <c>[$Name] : where Expr</c> 或无条件模板 <c>[$Name]</c>。
        /// where 表达式作为原始字符串保留（TODO.md §4.3），不做求值；求值与应用由后续
        /// TemplateExpansionPass 完成。
        /// </summary>
        private TemplateNode ParseTemplateSection(
            IniToken open,
            IniToken? close,
            NamedText rawName,
            IniToken? colon,
            List<Trivia> leading,
            bool colonFollowedByWhere,
            bool colonFollowedByInheritance)
        {
            // 去掉 '$' 前缀作为模板名；NameSpan 仍然指向源文本（含 '$'）。
            string templateName = rawName.Text.Length > 0 && rawName.Text[0] is '$'
                ? rawName.Text[1..]
                : rawName.Text;

            string? whereExpr = null;
            SourceSpan whereSpan = SourceSpan.At(_path, close?.Span.End ?? rawName.Span.End);

            if (colon is not null && colonFollowedByWhere)
            {
                // 消费 "where" 关键字。
                SkipWhitespace();
                IniToken whereToken = Advance(); // Text("where")
                int whereStart = whereToken.Span.End;

                // 收集行内剩余所有记号（直到 Newline 或 EOF）作为 where 表达式的原始文本。
                List<IniToken> inline = [];
                while (Current.Type is not (IniTokenType.Newline or IniTokenType.EndOfFile))
                    inline.Add(Advance());

                // 裁剪首尾空白后拼成原始表达式。
                int first = 0;
                while (first < inline.Count && IsBlank(inline[first]))
                    first++;
                int last = inline.Count;
                while (last > first && IsBlank(inline[last - 1]))
                    last--;

                if (first >= last)
                {
                    Report(IniDiagnostics.TemplateWhereMissing(whereToken.Span));
                    whereSpan = new(_path, whereStart, whereStart);
                }
                else
                {
                    whereExpr = string.Concat(inline[first..last].Select(static token => token.Text));
                    whereSpan = new(_path, inline[first].Span.Start, inline[last - 1].Span.End);
                }
            }
            else if (colon is not null && colonFollowedByInheritance is false && colonFollowedByWhere is false)
            {
                // ':' 之后既不是 '[' 也不是 'where'。
                Report(IniDiagnostics.TemplateWhereExpected(Current.Span));
            }

            List<Trivia> trailing = CollectRestOfLine();
            List<KeyValueNode> keyValues = ParseBody();

            SourceSpan header;
            if (whereExpr is not null)
                header = new(_path, open.Span.Start, whereSpan.End);
            else if (close is not null)
                header = new(_path, open.Span.Start, close.Span.End);
            else
                header = new(_path, open.Span.Start, rawName.Span.End);

            return new TemplateNode
            {
                Name = templateName,
                WhereExpression = whereExpr,
                KeyValues = keyValues,
                LeadingTrivia = leading,
                TrailingTrivia = trailing,
                NameSpan = rawName.Span,
                WhereSpan = whereSpan,
                HeaderSpan = header,
                Span = new(_path, open.Span.Start, header.End),
            };
        }

        /// <summary>
        /// 解析节体：连续的键值对，直到遇到下一个节头或 EOF。
        /// </summary>
        private List<KeyValueNode> ParseBody()
        {
            List<KeyValueNode> keyValues = [];
            while (Current.Type is not IniTokenType.EndOfFile
                && IsAtSectionBoundary() is false)
            {
                if (Current.IsKeyStart is false)
                {
                    Report(IniDiagnostics.UnexpectedToken(Current.Text, Current.Span));
                    Advance();
                    continue;
                }

                KeyValueNode? keyValue = ParseKeyValue(CollectTrivia());
                if (keyValue is not null)
                    keyValues.Add(keyValue);
            }

            return keyValues;
        }

        /// <summary>
        /// 判断当前位置（跳过空白之后）是否是一个独立的 Text 记号，其文本不区分大小写等于 "where"。
        /// </summary>
        private bool TryPeekWhereKeyword()
        {
            int offset = 0;
            while (Peek(offset).Type is IniTokenType.Whitespace)
                offset++;

            IniToken token = Peek(offset);
            return token.Type is IniTokenType.Text
                && token.Text.Equals("where", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 查询当前行偏移 <paramref name="lineOffset"/> 处生效的 <c>#pragma template</c> 状态：
        /// 取最后一个 <see cref="TemplateModeDirective.LineIndex"/> ≤ <paramref name="lineOffset"/> 的
        /// 指令的 <see cref="TemplateModeDirective.Enabled"/>；没有指令时默认 <see langword="true"/>（TODO.md §1.2.1）。
        /// </summary>
        private bool IsTemplateModeOn(int lineOffset)
        {
            bool on = true;
            foreach (var directive in _templateModes)
            {
                if (directive.LineIndex > lineOffset)
                    break;
                on = directive.Enabled;
            }

            return on;
        }

        /// <remarks>
        /// 第一项的分隔符总是 <see cref="SectionSeparator.Colon"/>，第二项及以后总是
        /// <see cref="SectionSeparator.Comma"/>；缺少期望的分隔符时分别报告 INI004/INI005，
        /// 但已识别出的继承项一律保留。
        /// </remarks>
        private List<SectionInheritance> ParseInheritanceList(IniToken? colon)
        {
            List<SectionInheritance> inheritances = [];
            if (colon is null)
                return inheritances;

            int start = colon.Span.Start;
            if (IsFollowedByToken(IniTokenType.LeftBracket) is false)
            {
                // 冒号之后没有继承项。
                Report(IniDiagnostics.MissingComma(colon.Span));
                return inheritances;
            }

            SkipWhitespace();
            Advance();
            inheritances.Add(ParseInheritance(inheritances.Count, start));
            SkipWhitespace();

            while (true)
            {
                // 逗号优先：先消费逗号再检查继承项，避免把 ", [" 中的方括号与逗号混为一谈。
                if (Current.Type is IniTokenType.Comma)
                {
                    IniToken comma = Advance();
                    start = comma.Span.Start;
                    SkipWhitespace();

                    if (Current.Type is IniTokenType.LeftBracket)
                    {
                        Advance();
                        inheritances.Add(ParseInheritance(inheritances.Count, start));
                        continue;
                    }

                    Report(IniDiagnostics.SyntaxError("逗号之后缺少继承项", comma.Span));
                    return inheritances;
                }

                if (IsFollowedByToken(IniTokenType.LeftBracket))
                {
                    // 两个继承项之间缺少逗号：先报告，再继续解析，避免后续项被拆散。
                    Report(IniDiagnostics.MissingComma(Current.Span));
                    SkipWhitespace();
                    Advance();
                    inheritances.Add(ParseInheritance(inheritances.Count, start));
                    continue;
                }

                return inheritances;
            }
        }

        /// <summary>
        /// 判断当前位置是否已到达下一个节的节头（跳过其间的前导 trivia）；文件尾 trivia 因此不会被
        /// 并入上一个节，而是留给文档尾随 trivia。
        /// </summary>
        private bool IsAtSectionBoundary()
        {
            int offset = 0;
            while (Peek(offset).Type is IniTokenType.Whitespace or IniTokenType.Newline or IniTokenType.Hash or IniTokenType.Semicolon)
                offset++;

            return Peek(offset).Type is IniTokenType.LeftBracket;
        }

        /// <summary>
        /// 跳过当前连续的空白记号。
        /// </summary>
        private void SkipWhitespace()
        {
            while (Current.Type is IniTokenType.Whitespace)
                Advance();
        }

        /// <summary>
        /// 解析一个继承项：空白 + <c>[名称]</c>。名称之后允许再次出现空白（<c>[A] , [B]</c>），
        /// 由调用方负责跳过；只有名称之后、之内的空白才进入名称范围的计算。
        /// </summary>
        private SectionInheritance ParseInheritance(int index, int start)
        {
            SkipWhitespace();
            NamedText name = ReadName();
            SkipWhitespace();

            bool hasCloseBracket = TryMatch(IniTokenType.RightBracket, out IniToken close);
            if (hasCloseBracket is false)
                Report(IniDiagnostics.MissingRightBracket(Current.Span));

            if (name.Text.Length is 0)
                Report(IniDiagnostics.MissingName(name.Span));

            return new()
            {
                Name = name.Text,
                HasOpenBracket = true,
                HasCloseBracket = hasCloseBracket,
                Separator = index is 0 ? SectionSeparator.Colon : SectionSeparator.Comma,
                Span = new(_path, start, hasCloseBracket ? close.Span.End : name.Span.End),
            };
        }

        private KeyValueNode? ParseKeyValue(List<Trivia> leading)
        {
            int start = Current.Span.Start;
            NamedText key = ReadKey();

            if (key.Text.Length is 0)
            {
                // 没有键：整行都不是合法的键值对，报告一次 INI007 后丢弃该行。
                Report(IniDiagnostics.MissingKey(key.Span));
                SkipLine();
                return null;
            }

            if (key.Text.AsSpan().IndexOfAny(' ', '\t') >= 0)
                Report(IniDiagnostics.WhitespaceInKey(key.Text, key.Span));

            while (Current.Type is IniTokenType.Whitespace)
                Advance();

            bool hasEquals = TryMatch(IniTokenType.EqualSign, out IniToken equals);
            if (hasEquals is false)
                Report(IniDiagnostics.MissingEquals(key.Text, Current.Span));

            List<IniToken> inline = [];
            while (Current.Type is not (IniTokenType.Newline or IniTokenType.EndOfFile))
                inline.Add(Advance());

            List<Trivia> trailing = CollectTrivia();
            NamedText value = MakeValue(inline, hasEquals ? equals.Span.End : start);

            if (hasEquals is false)
                return null;

            if (value.Text.Length is 0)
                Report(IniDiagnostics.MissingValue(value.Span));

            return new()
            {
                Key = key.Text,
                Value = value.Text,
                HasEquals = true,
                LeadingTrivia = leading,
                TrailingTrivia = trailing,
                KeySpan = key.Span,
                ValueSpan = value.Span,
                Span = new(_path, start, Math.Max(key.Span.End, value.Span.End)),
            };
        }

        /// <summary>
        /// 由行内记号构造值：所有记号按原文拼接，在首个非空白记号处开始、在最后一个非空白记号处结束；
        /// 注释在 MVP 中按原文保留，不被解码或剥离。
        /// </summary>
        private NamedText MakeValue(List<IniToken> inline, int emptyAt)
        {
            if (inline.Count is 0)
                return new(string.Empty, new(_path, emptyAt, emptyAt));

            int first = 0;
            while (first < inline.Count && IsBlank(inline[first]))
                first++;

            int last = inline.Count;
            while (last > first && IsBlank(inline[last - 1]))
                last--;

            if (first >= last)
                return new(string.Empty, new(_path, emptyAt, emptyAt));

            string text = string.Concat(inline[first..last]
                .Select(static token => token.Text));

            return new(text, new(_path, inline[first].Span.Start, inline[last - 1].Span.End));
        }

        /// <summary>
        /// 读取键：只接受单个 <see cref="IniTokenType.Text"/>，或 <c>:</c> 加单个
        /// <see cref="IniTokenType.Text"/>（如 <c>:SecretBuilding</c>）。
        /// 键后直接跟随 <c>Text</c> 片段（即相隔空白之外还有词）时，把这些片段与空白一起并入键，
        /// 由 INI009 提示“键含空白”，以保证 <c>My Key=1</c> 这类写法的值不会被误当作键的一部分。
        /// 键以 <c>:</c> 开头却不跟文本（如 <c>:=1</c> 的 <c>:</c>）或紧邻特殊记号（如 <c>[</c>）时，视为没有键。
        /// </summary>
        private NamedText ReadKey()
        {
            int start = Current.Span.Start;
            if (Current.IsKeyStart is false)
                return new(string.Empty, SourceSpan.At(_path, start));

            if (Current.Type is IniTokenType.EqualSign)
                return new(string.Empty, new(_path, start, Current.Span.End));

            IniToken first = Advance();
            if (first.Type is IniTokenType.Colon)
            {
                // 冒号必须紧邻文本才能构成键，例如 :SecretBuilding。
                if (Current.Type is not IniTokenType.Text)
                    return new(string.Empty, new(_path, start, first.Span.End));

                IniToken name = Advance();
                return new($"{first.Text}{name.Text}", new(_path, start, name.Span.End));
            }

            if (Current.Type is not IniTokenType.Whitespace)
                return new(first.Text, first.Span);

            int whitespaceStart = Current.Span.Start;
            List<IniToken> pieces = [first];
            while (Current.Type is IniTokenType.Text or IniTokenType.Whitespace)
                pieces.Add(Advance());

            if (pieces.Count is 1)
                return new(first.Text, first.Span);

            // 空白之后仍是文本，说明键或值本身含有空白：把它们并回键，由 INI009 提示。
            string text = string.Concat(pieces.Select(static token => token.Text));
            return text.Length is 0
                ? new(string.Empty, new(_path, whitespaceStart, whitespaceStart))
                : new(text.Trim(), new(_path, first.Span.Start, pieces[^1].Span.End));
        }

        private NamedText ReadName()
        {
            int start = Current.Span.Start;
            List<IniToken> pieces = [];
            while (Current.Type is IniTokenType.Text or IniTokenType.Whitespace)
                pieces.Add(Advance());

            if (pieces.Count is 0)
                return new(string.Empty, new(_path, start, start));

            int first = 0;
            while (first < pieces.Count && IsBlank(pieces[first]))
                first++;

            int last = pieces.Count;
            while (last > first && IsBlank(pieces[last - 1]))
                last--;

            if (first >= last)
                return new(string.Empty, new(_path, start, start));

            string text = string.Concat(pieces[first..last]
                .Select(static token => token.Text));

            return new(text, new(_path, pieces[first].Span.Start, pieces[last - 1].Span.End));
        }

        private bool IsFollowedByToken(IniTokenType type)
        {
            int offset = 0;
            while (Peek(offset).Type is IniTokenType.Whitespace)
                offset++;

            return Peek(offset).Type == type;
        }

        private SourceSpan MakeHeaderSpan(int start, IniToken? close, List<SectionInheritance> inheritances)
        {
            if (inheritances is { Count: > 0 })
            {
                SectionInheritance last = inheritances[^1];
                if (last.HasCloseBracket)
                    return new(_path, start, last.Span.End);
            }

            if (close is not null)
                return new(_path, start, close.Span.End);

            int end = Current.Span.Start;
            return end > start ? new(_path, start, end) : SourceSpan.At(_path, start);
        }

        private IniToken Peek(int ahead)
        {
            int index = _position + ahead;
            return index < _tokens.Count ? _tokens[index] : _tokens[^1];
        }

        private IniToken Previous
            => _tokens[_position > 0 ? _position - 1 : 0];

        private IniToken Advance()
        {
            IniToken token = Current;
            if (_position < _tokens.Count)
                _position++;

            return token;
        }

        private bool TryMatch(IniTokenType type, out IniToken token)
        {
            if (Current.Type != type)
            {
                token = Current;
                return false;
            }

            token = Advance();
            return true;
        }

        private List<Trivia> CollectTrivia()
        {
            List<Trivia> trivia = [];
            while (TryMakeTrivia(Current, out Trivia? item))
            {
                Advance();
                trivia.Add(item);
            }

            return trivia;
        }

        /// <summary>
        /// 收集当前位置到行尾（不含换行）的自由记号；换行也一并收集。
        /// </summary>
        private List<Trivia> CollectRestOfLine()
            => CollectTrivia();

        /// <summary>
        /// 丢弃当前行的剩余记号（直到并包含换行）。
        /// </summary>
        private void SkipLine()
        {
            while (Current.Type is not (IniTokenType.Newline or IniTokenType.EndOfFile))
                Advance();

            if (Current.Type is IniTokenType.Newline)
                Advance();
        }

        private static bool TryMakeTrivia(
            IniToken token,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Trivia? trivia)
        {
            TriviaKind? kind = token.Type switch
            {
                IniTokenType.Hash => TriviaKind.HashComment,
                IniTokenType.Semicolon => TriviaKind.SemicolonComment,
                IniTokenType.Whitespace => TriviaKind.Whitespace,
                IniTokenType.Newline => TriviaKind.Newline,
                _ => null,
            };

            trivia = kind is null ? null : new(kind.Value, token.Text, token.Span);
            return trivia is not null;
        }

        private static bool IsBlank(IniToken token)
            => token.Type is IniTokenType.Whitespace or IniTokenType.Newline or IniTokenType.EndOfFile;

        private void Report(Diagnostic diagnostic)
            => _diagnostics.Add(diagnostic);
    }
}
