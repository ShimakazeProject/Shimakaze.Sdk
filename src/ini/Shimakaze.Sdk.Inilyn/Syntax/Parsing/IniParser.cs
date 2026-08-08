using System.Diagnostics.CodeAnalysis;
using System.Text;

using Shimakaze.Sdk.Inilyn.Lexer;
using Shimakaze.Sdk.Inilyn.Syntax.Nodes;

namespace Shimakaze.Sdk.Inilyn.Syntax.Parsing;

/// <summary>
/// INI 语法解析器。
/// </summary>
/// <remarks>
/// <para>
/// 将 <see cref="IniToken"/> 流组合为结构化的语法树。
/// </para>
/// <para>
/// 语法 BNF：
/// <code>
/// compilation-unit  ::= entry* eof
/// entry             ::= section-declaration | key-value-entry | trivia
/// section-declaration ::= trivia* '[' string-token ']' ( ':' mixin-ref-list )? newline entry*
/// mixin-ref-list    ::= mixin-ref ( ',' mixin-ref )*
/// mixin-ref         ::= trivia* '[' string-token ']'
/// key-value-entry   ::= trivia* string-token '=' string-token trivia* newline
/// trivia            ::= comment | doc-comment | whitespace
/// </code>
/// </para>
/// </remarks>
public sealed class IniParser(IReadOnlyList<IniToken> tokens)
{
    private readonly List<Diagnostic> _diagnostics = [];
    private int _position;

    /// <summary>
    /// 对给定的 INI 文本进行语法分析。
    /// </summary>
    /// <param name="content">INI 文本内容。</param>
    /// <returns>语法树。</returns>
    public static IniSyntaxTree Parse(string content)
    {
        List<IniToken> tokens = [.. IniLexer.Tokenize(content)];
        IniParser parser = new(tokens);
        return parser.Parse();
    }

    /// <summary>
    /// 对给定的 <see cref="Text.SourceText"/> 进行语法分析。
    /// </summary>
    /// <param name="sourceText">源文本。</param>
    /// <returns>语法树。</returns>
    public static IniSyntaxTree Parse(Text.SourceText sourceText)
    {
        List<IniToken> tokens = [.. IniLexer.Tokenize(sourceText.ToString())];
        IniParser parser = new(tokens);
        var tree = parser.Parse();
        return new IniSyntaxTree(sourceText, tree.Root, tree.Diagnostics);
    }

    /// <summary>
    /// 执行语法分析。
    /// </summary>
    /// <returns>语法树。</returns>
    public IniSyntaxTree Parse()
    {
        int start = CurrentToken().Offset;
        List<IniSyntaxNode> entries = [];

        while (!IsEndOfFile())
        {
            var entry = ParseEntry();
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        var eof = ConvertToken(CurrentToken());
        int end = eof.End;

        IniCompilationUnit unit = new(entries, eof, start, end);
        return new IniSyntaxTree(
            Text.SourceText.Create(string.Empty),
            unit,
            [.. _diagnostics]
        );
    }

    private IniSyntaxNode? ParseEntry()
    {
        // 跳过 trivia
        var leadingTrivia = CollectTrivia();
        var current = CurrentToken();

        // 节声明：[ ... ]
        if (current.Type == IniTokenType.LeftBracket)
        {
            return ParseSectionDeclaration(leadingTrivia);
        }

        // 预处理指令
        if (current.Type == IniTokenType.PreprocessorDirective)
        {
            return ParsePreprocessorDirective();
        }

        // 键值对：string = string（键可以以冒号开头）
        if (current.Type == IniTokenType.Text || current.Type == IniTokenType.Colon)
        {
            return ParseKeyValueEntry(leadingTrivia);
        }

        // 不可识别的 token：跳过并报告错误
        if (current.Type != IniTokenType.EndOfFile)
        {
            var (endLine, endColumn) = EndPosition(current);
            ReportDiagnostic(Diagnostic.Create(Diagnostics.UnexpectedToken, current.Line, current.Column, endLine, endColumn, null, current.Text));
            Advance();
        }

        return null;
    }

    private IniSectionDecl ParseSectionDeclaration(List<IniSyntaxNode> leadingTrivia)
    {
        int start = CurrentToken().Offset;

        var leftBracket = ConvertToken(Advance());

        // 节名（允许包含空格）
        var name = ReadSectionName();

        // 右方括号
        IniSyntaxToken rightBracket;
        if (TryMatch(IniTokenType.RightBracket, out var rbToken))
        {
            rightBracket = ConvertToken(rbToken);
        }
        else
        {
            var current = CurrentToken();
            var (endLine, endColumn) = EndPosition(current);
            ReportDiagnostic(Diagnostic.Create(Diagnostics.ExpectedRightBracket, current.Line, current.Column, endLine, endColumn));
            rightBracket = MakeMissingToken(IniSyntaxKind.RightBracketToken, current);
        }

        // 可选的 Mixin 子句（冒号前允许空白）
        IniMixinClause? mixinClause = null;
        SkipWhitespaceTrivia();
        if (TryPeek(IniTokenType.Colon))
        {
            mixinClause = ParseMixinClause();
        }

        // 节声明行剩余部分全部视为 trivia（如 [Name] // 注释）
        var trailingTrivia = CollectRestOfLineTrivia();

        // 解析节内的子条目
        List<IniSyntaxNode> children = [];
        while (!IsEndOfFile() && !IsAtSectionStart())
        {
            var child = ParseEntry();
            if (child is not null)
            {
                children.Add(child);
            }
        }

        int end = children.Count > 0 ? children[^1].End : (mixinClause?.End ?? rightBracket.End);
        return new IniSectionDecl(
            leadingTrivia, leftBracket, name, rightBracket,
            mixinClause, trailingTrivia, children,
            start, end
        );
    }

    private IniMixinClause ParseMixinClause()
    {
        int start = CurrentToken().Offset;
        var colon = ConvertToken(Advance());

        List<IniMixinReference> references = [];
        while (true)
        {
            SkipWhitespaceTrivia();
            if (!TryPeek(IniTokenType.LeftBracket))
            {
                break;
            }

            references.Add(ParseMixinReference());

            SkipWhitespaceTrivia();
            if (TryPeek(IniTokenType.Comma))
            {
                Advance();
                continue;
            }

            break;
        }

        int end = references.Count > 0 ? references[^1].End : colon.End;
        return new IniMixinClause(colon, [.. references], start, end);
    }

    private IniMixinReference ParseMixinReference()
    {
        int start = CurrentToken().Offset;
        var leadingTrivia = CollectTrivia();
        var leftBracket = ConvertToken(Advance());

        // 引用的节名（允许包含空格）
        var name = ReadSectionName(isMixin: true);

        IniSyntaxToken rightBracket;
        if (TryMatch(IniTokenType.RightBracket, out var rbToken))
        {
            rightBracket = ConvertToken(rbToken);
        }
        else
        {
            var current = CurrentToken();
            var (endLine, endColumn) = EndPosition(current);
            ReportDiagnostic(Diagnostic.Create(Diagnostics.ExpectedMixinRightBracket, current.Line, current.Column, endLine, endColumn));
            rightBracket = MakeMissingToken(IniSyntaxKind.RightBracketToken, current);
        }

        int end = rightBracket.End;
        return new IniMixinReference(leadingTrivia, leftBracket, name, rightBracket, start, end);
    }

    private IniKeyValueEntry? ParseKeyValueEntry(List<IniSyntaxNode> leadingTrivia)
    {
        int start = CurrentToken().Offset;

        // 键（允许以冒号开头，如 :SecretBuilding=）
        var key = ReadKeyToken();

        // 键与 '=' 之间允许空白
        SkipWhitespaceTrivia();

        // 等号
        IniSyntaxToken equals;
        if (TryMatch(IniTokenType.EqualSign, out var eqToken))
        {
            equals = ConvertToken(eqToken);
        }
        else
        {
            // 没有 '=' 的行不是合法键值对，跳过整行（与游戏行为一致）
            var current = CurrentToken();
            var (endLine, endColumn) = EndPosition(current);
            ReportDiagnostic(Diagnostic.Create(Diagnostics.ExpectedEqualSign, current.Line, current.Column, endLine, endColumn));
            CollectRestOfLineTrivia();
            return null;
        }

        // 值（词法器将 '=' 后整行内容作为单个 String 记号，无需跳过空白）
        IniSyntaxToken value;
        if (TryMatch(IniTokenType.Text, out var valToken))
        {
            value = ConvertToken(valToken);
        }
        else
        {
            var current = CurrentToken();
            var (endLine, endColumn) = EndPosition(current);
            ReportDiagnostic(Diagnostic.Create(Diagnostics.ExpectedValue, current.Line, current.Column, endLine, endColumn));
            value = MakeMissingToken(IniSyntaxKind.StringToken, current);
        }

        // 尾随 trivia
        var trailingTrivia = CollectTriviaUntilNewline();

        int end = value.End;
        return new IniKeyValueEntry(leadingTrivia, key, equals, value, trailingTrivia, start, end);
    }

    private IniPreprocessorDirective ParsePreprocessorDirective()
    {
        int start = CurrentToken().Offset;
        var token = ConvertToken(Advance());
        int end = token.End;

        // 预处理指令后面可能有换行 trivia
        return new IniPreprocessorDirective(token, start, end);
    }

    private List<IniSyntaxNode> CollectTrivia()
    {
        List<IniSyntaxNode> trivia = [];
        while (IsTrivia())
        {
            trivia.Add(ConvertTrivia(Advance()));
        }

        return trivia;
    }

    private List<IniSyntaxNode> CollectTriviaUntilNewline()
    {
        List<IniSyntaxNode> trivia = [];
        while (IsTrivia() && CurrentToken().Type != IniTokenType.Newline)
        {
            trivia.Add(ConvertTrivia(Advance()));
        }

        // 消费换行符
        if (TryMatch(IniTokenType.Newline, out var newlineToken))
        {
            trivia.Add(ConvertTrivia(newlineToken));
        }

        return trivia;
    }

    private void SkipWhitespaceTrivia()
    {
        while (CurrentToken().Type == IniTokenType.Whitespace)
        {
            Advance();
        }
    }

    private List<IniSyntaxNode> CollectRestOfLineTrivia()
    {
        List<IniSyntaxNode> trivia = [];
        while (CurrentToken().Type != IniTokenType.Newline && CurrentToken().Type != IniTokenType.EndOfFile)
        {
            trivia.Add(ConvertTrivia(Advance()));
        }

        // 消费换行符
        if (TryMatch(IniTokenType.Newline, out var newlineToken))
        {
            trivia.Add(ConvertTrivia(newlineToken));
        }

        return trivia;
    }

    private IniSyntaxToken ReadSectionName(bool isMixin = false)
    {
        int start = CurrentToken().Offset;
        StringBuilder builder = new();
        while (CurrentToken().Type is IniTokenType.Text or IniTokenType.Whitespace)
        {
            builder.Append(CurrentToken().Text);
            Advance();
        }

        string text = builder.ToString().Trim();
        if (text.Length == 0)
        {
            var current = CurrentToken();
            var (endLine, endColumn) = EndPosition(current);
            var diagnostic = isMixin
                ? Diagnostic.Create(Diagnostics.ExpectedMixinSectionName, current.Line, current.Column, endLine, endColumn)
                : Diagnostic.Create(Diagnostics.ExpectedSectionName, current.Line, current.Column, endLine, endColumn);
            ReportDiagnostic(diagnostic);
            return MakeMissingToken(IniSyntaxKind.StringToken, current);
        }

        return new IniSyntaxToken(IniSyntaxKind.StringToken, text, start, start + text.Length);
    }

    private IniSyntaxToken ReadKeyToken()
    {
        int start = CurrentToken().Offset;
        StringBuilder builder = new();

        if (TryMatch(IniTokenType.Colon, out _))
        {
            builder.Append(':');
        }

        if (TryMatch(IniTokenType.Text, out var keyToken))
        {
            builder.Append(keyToken.Text);
        }

        string text = builder.ToString();
        if (text.Length == 0)
        {
            return MakeMissingToken(IniSyntaxKind.StringToken, CurrentToken());
        }

        return new IniSyntaxToken(IniSyntaxKind.StringToken, text, start, start + text.Length);
    }

    private bool IsTrivia()
    {
        var type = CurrentToken().Type;
        return type == IniTokenType.Whitespace
            || type == IniTokenType.Comment
            || type == IniTokenType.DocComment;
    }

    private bool IsAtSectionStart()
    {
        var current = CurrentToken();
        if (current.Type == IniTokenType.LeftBracket)
        {
            return true;
        }

        // 可能有前导 trivia，跳过检查
        int saved = _position;
        while (_position < tokens.Count && IsTriviaAt(_position))
        {
            _position++;
        }

        bool result = _position < tokens.Count && tokens[_position].Type == IniTokenType.LeftBracket;
        _position = saved;
        return result;
    }

    private bool IsTriviaAt(int position) => tokens[position].Type is IniTokenType.Whitespace or IniTokenType.Comment or IniTokenType.DocComment;

    private IniToken CurrentToken()
    {
        if (_position < tokens.Count)
        {
            return tokens[_position];
        }

        return new IniToken(IniTokenType.EndOfFile, 0, 0, 0, string.Empty);
    }

    private IniToken Advance()
    {
        var token = CurrentToken();
        _position++;
        return token;
    }

    private bool TryMatch(IniTokenType type, [NotNullWhen(true)] out IniToken? token)
    {
        if (CurrentToken().Type == type)
        {
            token = Advance();
            return true;
        }

        token = null;
        return false;
    }

    private bool TryPeek(IniTokenType type) => CurrentToken().Type == type;

    private bool IsEndOfFile() => CurrentToken().Type == IniTokenType.EndOfFile;

    private void ReportDiagnostic(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }

    private static (int Line, int Column) EndPosition(IniToken token)
    {
        int lineBreaks = 0;
        int lastBreak = -1;
        for (int i = 0; i < token.Text.Length; i++)
        {
            if (token.Text[i] == '\n')
            {
                lineBreaks++;
                lastBreak = i;
            }
        }

        if (lineBreaks == 0)
        {
            return (token.Line, token.Column + token.Text.Length);
        }

        return (token.Line + lineBreaks, token.Text.Length - (lastBreak + 1) + 1);
    }

    private static IniSyntaxToken ConvertToken(IniToken token)
    {
        var kind = token.Type switch
        {
            IniTokenType.LeftBracket => IniSyntaxKind.LeftBracketToken,
            IniTokenType.RightBracket => IniSyntaxKind.RightBracketToken,
            IniTokenType.EqualSign => IniSyntaxKind.EqualToken,
            IniTokenType.Colon => IniSyntaxKind.ColonToken,
            IniTokenType.Comma => IniSyntaxKind.CommaToken,
            IniTokenType.Comment => IniSyntaxKind.CommentTrivia,
            IniTokenType.DocComment => IniSyntaxKind.DocCommentTrivia,
            IniTokenType.Whitespace => IniSyntaxKind.WhitespaceTrivia,
            IniTokenType.Text => IniSyntaxKind.StringToken,
            IniTokenType.PreprocessorDirective => IniSyntaxKind.PreprocessorDirectiveToken,
            IniTokenType.Newline => IniSyntaxKind.NewlineTrivia,
            IniTokenType.EndOfFile => IniSyntaxKind.EndOfFileToken,
            _ => IniSyntaxKind.BadToken,
        };

        return new IniSyntaxToken(kind, token.Text, token.Offset, token.Offset + token.Text.Length);
    }

    private static IniTriviaNode ConvertTrivia(IniToken token)
    {
        var kind = token.Type switch
        {
            IniTokenType.Whitespace => IniSyntaxKind.WhitespaceTrivia,
            IniTokenType.Comment => IniSyntaxKind.CommentTrivia,
            IniTokenType.DocComment => IniSyntaxKind.DocCommentTrivia,
            IniTokenType.Newline => IniSyntaxKind.NewlineTrivia,
            _ => IniSyntaxKind.WhitespaceTrivia,
        };

        return new IniTriviaNode(kind, token.Text, token.Offset, token.Offset + token.Text.Length);
    }

    private static IniSyntaxToken MakeMissingToken(IniSyntaxKind kind, IniToken atPosition)
        => new(kind, string.Empty, atPosition.Offset, atPosition.Offset);
}
