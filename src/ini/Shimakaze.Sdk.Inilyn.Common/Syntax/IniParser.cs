using System.Text;

using Shimakaze.Sdk.Inilyn.Data;
using Shimakaze.Sdk.Inilyn.Data.Lexer;
using Shimakaze.Sdk.Inilyn.Data.Syntax;

using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// INI 语法解析器（数据库版本）。
/// </summary>
/// <remarks>
/// <para>
/// 从数据库读取 <see cref="IniToken"/> 流，生成数据库实体形式的语法树节点和诊断信息。
/// </para>
/// <para>
/// 语法 BNF：
/// <code>
/// compilation-unit  ::= entry* eof
/// entry             ::= section-declaration | key-value-entry | trivia
/// section-declaration ::= trivia* '[' string ']' ( ':' inheritance-list )? newline entry*
/// inheritance-list  ::= inheritance ( ',' inheritance )*
/// inheritance       ::= trivia* '[' string ']'
/// key-value-entry   ::= trivia* string '=' value trivia* newline
/// value             ::= 到行尾/注释/EOF 之前的所有内容
/// </code>
/// </para>
/// </remarks>
/// <remarks>
/// 初始化 <see cref="IniParser"/> 的新实例。
/// </remarks>
/// <param name="db">数据库上下文。</param>
/// <param name="documentId">文档 ID。</param>
public sealed class IniParser(IniDbContext db, Guid documentId)
{
    private readonly IniDbContext _db = db;
    private readonly List<IniToken> _tokens = [.. db.Tokens
            .Where(t => t.DocumentId == documentId)
            .OrderBy(t => t.Order)];
    private readonly List<IniDiagnostic> _diagnostics = [];
    private int _position;
    private int _nodeOrder;

    /// <summary>
    /// 创建一个 <see cref="LsRange"/>。
    /// </summary>
    private static LsRange MakeLsRange(uint startLine, uint startChar, uint endLine, uint endChar)
        => new() { Start = new() { Line = startLine, Character = startChar }, End = new() { Line = endLine, Character = endChar } };

    /// <summary>
    /// 从 <see cref="IniToken"/> 的位置构造一个 <see cref="LsRange"/>。
    /// </summary>
    private static LsRange TokenRange(IniToken token) => token.Position;

    /// <summary>
    /// 执行语法分析并将结果保存到数据库。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task ParseAsync(CancellationToken cancellationToken = default)
    {
        while (!IsEndOfFile())
        {
            var entry = ParseEntry();
            if (entry is SectionNode section)
            {
                _db.Sections.Add(section);
            }
        }

        if (_diagnostics.Count > 0)
        {
            _db.Diagnostics.AddRange(_diagnostics);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private SectionNode? ParseEntry()
    {
        // 跳过 trivia 并保存
        CollectTrivia(attachedToNodeId: null);

        var current = CurrentToken();

        // 节声明：[ ... ]
        if (current.Type == IniTokenType.LeftBracket)
            return ParseSectionDeclaration();

        // 顶层键值对：无所属节，诊断后跳过
        if (current.Type is IniTokenType.Text or IniTokenType.Colon)
        {
            var kvp = ParseKeyValueEntry(sectionId: null);
            return null; // 不存入数据库（无所属节）
        }

        // 不可识别的 token：跳过并报告错误
        if (current.Type != IniTokenType.EndOfFile)
        {
            ReportDiagnostic(Diagnostics.INI099, TokenRange(current), current.Text);
            Advance();
        }

        return null;
    }

    private void ParseEntryInSection(Guid sectionId)
    {
        // 跳过 trivia 并保存
        CollectTrivia(attachedToNodeId: null);

        var current = CurrentToken();

        // 节声明：遇到新节头，返回让外层循环处理
        if (current.Type == IniTokenType.LeftBracket)
            return;

        // 键值对：string = string
        if (current.Type is IniTokenType.Text or IniTokenType.Colon)
        {
            ParseKeyValueEntry(sectionId: sectionId);
            return;
        }

        // 不可识别的 token：跳过并报告错误
        if (current.Type != IniTokenType.EndOfFile)
        {
            ReportDiagnostic(Diagnostics.INI099, TokenRange(current), current.Text);
            Advance();
        }
    }

    private SectionNode ParseSectionDeclaration()
    {
        var leftBracket = CurrentToken();
        bool hasOpenBracket = TryMatch(IniTokenType.LeftBracket, out _);

        // 节名（允许包含空格）
        var (name, nameRange) = ReadSectionName();

        // 右方括号
        bool hasCloseBracket = TryMatch(IniTokenType.RightBracket, out _);
        if (!hasCloseBracket)
        {
            var pos = CurrentToken().Position.Start;
            ReportDiagnostic(Diagnostics.INI002, MakeLsRange(pos.Line, pos.Character, pos.Line, pos.Character));
        }

        // 可选的继承子句（冒号前允许空白）
        SkipWhitespaceTrivia();
        List<SectionInheritance> inheritances = [];
        if (TryPeek(IniTokenType.Colon))
        {
            ParseInheritanceClause(inheritances);
        }

        // 节声明行剩余部分全部视为 trivia
        CollectRestOfLineTrivia(attachedToNodeId: null);

        // 创建 SectionNode
        SectionNode section = new()
        {
            Id = Guid.CreateVersion7(),
            DocumentId = documentId,
            Order = _nodeOrder++,
            Range = TokenRange(leftBracket),
            Name = name,
            HasOpenBracket = hasOpenBracket,
            HasCloseBracket = hasCloseBracket,
            Inheritances = inheritances,
        };
        _db.Sections.Add(section);

        // 名称缺失诊断
        if (string.IsNullOrEmpty(name))
            ReportDiagnostic(Diagnostics.INI003, nameRange);

        // 解析节内的子条目
        while (!IsEndOfFile() && !IsAtSectionStart())
        {
            ParseEntryInSection(section.Id);
        }

        return section;
    }

    private void ParseInheritanceClause(List<SectionInheritance> inheritances)
    {
        // 消费冒号
        Advance();

        while (true)
        {
            SkipWhitespaceTrivia();
            if (!TryPeek(IniTokenType.LeftBracket))
                break;

            var inheritLeftBracket = CurrentToken();
            Advance();

            var (inheritName, inheritNameRange) = ReadSectionName();

            var separator = SectionSeparator.None;
            if (TryMatch(IniTokenType.RightBracket, out _))
            {
                // 正常的 [name]
            }
            else
            {
                ReportDiagnostic(Diagnostics.INI002, inheritNameRange);
            }

            SectionInheritance inheritance = new()
            {
                Id = Guid.CreateVersion7(),
                Name = inheritName,
                Range = TokenRange(inheritLeftBracket),
                Separator = separator,
            };
            inheritances.Add(inheritance);

            if (string.IsNullOrEmpty(inheritName))
                ReportDiagnostic(Diagnostics.INI003, inheritNameRange);

            SkipWhitespaceTrivia();
            if (TryPeek(IniTokenType.Comma))
            {
                Advance();
                continue;
            }

            break;
        }
    }

    private KeyValuePairNode? ParseKeyValueEntry(Guid? sectionId)
    {
        // 键（允许以冒号开头，如 :SecretBuilding=）
        var (key, keyRange) = ReadKeyToken();

        // 键与 '=' 之间允许空白
        SkipWhitespaceTrivia();

        // 等号
        bool hasEquals = TryMatch(IniTokenType.EqualSign, out _);

        // 值：收集到行尾/注释/EOF 之前的所有内容
        StringBuilder valueBuilder = new();
        var curStart = CurrentToken().Position.Start;
        var valueRange = MakeLsRange(curStart.Line, curStart.Character, curStart.Line, curStart.Character);
        while (CurrentToken().Type is not (IniTokenType.Newline or IniTokenType.EndOfFile
            or IniTokenType.Hash or IniTokenType.Semicolon))
        {
            var vt = Advance();
            valueBuilder.Append(vt.Text);
            valueRange = MakeLsRange(
                valueRange.Start.Line, valueRange.Start.Character,
                vt.Position.End.Line, vt.Position.End.Character);
        }

        string value = valueBuilder.ToString().Trim();
        bool hasValue = value.Length > 0;

        // 尾随 trivia（注释）
        CollectTrivia(attachedToNodeId: null);

        // 诊断
        if (string.IsNullOrEmpty(key))
        {
            ReportDiagnostic(Diagnostics.INI007, keyRange);
            return null;
        }

        if (!hasEquals)
        {
            ReportDiagnostic(Diagnostics.INI006, keyRange);
            return null;
        }

        if (!hasValue)
            ReportDiagnostic(Diagnostics.INI008, valueRange);

        // 创建 KeyValuePairNode
        KeyValuePairNode kvp = new()
        {
            Id = Guid.CreateVersion7(),
            DocumentId = documentId,
            SectionId = sectionId,
            Order = _nodeOrder++,
            Range = keyRange,
            Key = key,
            HasEquals = hasEquals,
            Value = hasValue ? value : null,
        };
        _db.KeyValues.Add(kvp);

        return kvp;
    }

    #region Trivia

    private void CollectTrivia(Guid? attachedToNodeId)
    {
        while (IsTrivia())
        {
            var token = Advance();
            SaveTriviaToken(token, attachedToNodeId, isLeading: true);
        }
    }

    private void CollectRestOfLineTrivia(Guid? attachedToNodeId)
    {
        while (CurrentToken().Type is not (IniTokenType.Newline or IniTokenType.EndOfFile))
        {
            var token = Advance();
            SaveTriviaToken(token, attachedToNodeId, isLeading: false);
        }

        // 消费换行符
        if (TryMatch(IniTokenType.Newline, out var newlineToken))
            SaveTriviaToken(newlineToken!, attachedToNodeId, isLeading: false);
    }

    private void SaveTriviaToken(IniToken token, Guid? attachedToNodeId, bool isLeading)
    {
        TriviaKind? kind = token.Type switch
        {
            IniTokenType.Whitespace => TriviaKind.Whitespace,
            IniTokenType.Newline => TriviaKind.Newline,
            IniTokenType.Hash => TriviaKind.HashComment,
            IniTokenType.Semicolon => TriviaKind.SemicolonComment,
            _ => null,
        };

        if (kind.HasValue)
        {
            TriviaToken trivia = new()
            {
                Id = Guid.CreateVersion7(),
                DocumentId = documentId,
                Kind = kind.Value,
                Text = token.Text,
                AttachedToNodeId = attachedToNodeId,
                IsLeading = isLeading,
                Range = TokenRange(token),
            };
            _db.TriviaTokens.Add(trivia);
        }
    }

    private void SkipWhitespaceTrivia()
    {
        while (CurrentToken().Type == IniTokenType.Whitespace)
            Advance();
    }

    #endregion

    #region Name/Content Reading

    private (string Text, LsRange Range) ReadSectionName()
    {
        var startPos = CurrentToken().Position.Start;
        uint startLine = startPos.Line;
        uint startChar = startPos.Character;
        StringBuilder builder = new();
        var endRange = CurrentToken().Position;

        while (CurrentToken().Type is IniTokenType.Text or IniTokenType.Whitespace)
        {
            builder.Append(CurrentToken().Text);
            endRange = CurrentToken().Position;
            Advance();
        }

        string text = builder.ToString().Trim();
        var range = MakeLsRange(startLine, startChar, endRange.End.Line, endRange.End.Character);

        return (text, range);
    }

    private (string Text, LsRange Range) ReadKeyToken()
    {
        var startPos = CurrentToken().Position.Start;
        uint startLine = startPos.Line;
        uint startChar = startPos.Character;
        StringBuilder builder = new();
        var endRange = CurrentToken().Position;

        // 可选的冒号前缀
        if (TryMatch(IniTokenType.Colon, out var colonToken))
        {
            builder.Append(':');
            endRange = colonToken!.Position;
        }

        if (TryMatch(IniTokenType.Text, out var keyToken))
        {
            builder.Append(keyToken!.Text);
            endRange = keyToken.Position;
        }

        string text = builder.ToString();
        var range = MakeLsRange(startLine, startChar, endRange.End.Line, endRange.End.Character);

        return (text, range);
    }

    #endregion

    #region Token Navigation

    private bool IsTrivia()
        => CurrentToken().Type is IniTokenType.Whitespace or IniTokenType.Hash or IniTokenType.Semicolon or IniTokenType.Newline;

    private bool IsAtSectionStart()
    {
        if (CurrentToken().Type == IniTokenType.LeftBracket)
            return true;

        // 可能有前导 trivia，跳过检查
        int saved = _position;
        while (_position < _tokens.Count && IsTriviaAt(_position))
            _position++;

        bool result = _position < _tokens.Count && _tokens[_position].Type == IniTokenType.LeftBracket;
        _position = saved;
        return result;
    }

    private bool IsTriviaAt(int position)
        => _tokens[position].Type is IniTokenType.Whitespace or IniTokenType.Hash or IniTokenType.Semicolon or IniTokenType.Newline;

    private IniToken CurrentToken()
    {
        if (_position < _tokens.Count)
            return _tokens[_position];

        return new IniToken
        {
            Id = Guid.CreateVersion7(),
            DocumentId = documentId,
            Order = _tokens.Count,
            Type = IniTokenType.EndOfFile,
            Text = string.Empty,
            Position = MakeLsRange(0, 0, 0, 0),
        };
    }

    private IniToken Advance()
    {
        var token = CurrentToken();
        _position++;
        return token;
    }

    private bool TryMatch(IniTokenType type, out IniToken? token)
    {
        if (CurrentToken().Type == type)
        {
            token = Advance();
            return true;
        }

        token = null;
        return false;
    }

    private bool TryPeek(IniTokenType type)
        => CurrentToken().Type == type;

    private bool IsEndOfFile()
        => CurrentToken().Type == IniTokenType.EndOfFile;

    #endregion

    #region Diagnostics

    private void ReportDiagnostic(DiagnosticDescriptor descriptor, LsRange range, params object[] args)
    {
        IniDiagnostic diagnostic = new()
        {
            Id = Guid.CreateVersion7(),
            DocumentId = documentId,
            Code = descriptor.Id,
            Message = args.Length > 0
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, descriptor.MessageFormat, args)
                : descriptor.MessageFormat,
            Severity = descriptor.DefaultSeverity,
            Range = range,
        };
        _diagnostics.Add(diagnostic);
    }

    #endregion
}
