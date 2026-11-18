namespace Shimakaze.Sdk.Ra2lyn.Common.Template;

/// <summary>
/// <c>where</c> 表达式递归下降语法分析器（TODO.md §4.3.1）。
/// </summary>
/// <remarks>
/// 优先级由低到高：<c>||</c> → <c>&amp;&amp;</c> → 关系比较 → 一元 <c>!</c> → 链式访问 / 方法调用 / 原子。
/// <c>!</c> 的优先级高于比较，因此 <c>!(Techlevel &gt;= 0)</c> 需要括号；这与
/// <c>!null</c>（§4.3.5.3）作为独立的一元表达式成立是同一套规则。
/// <para/>
/// 解析成功不代表求值一定成功：NullReferenceError 之类的诊断只有在知道被评估节后才可能发生，
/// 因此求值期语义全部留在 <see cref="WhereExpressionEvaluator"/>。
/// </remarks>
public sealed class WhereParser
{
    private IReadOnlyList<WhereToken> _tokens = [];
    private int _position;

    /// <summary>
    /// 解析完整表达式（模板节 <c>where</c> 关键字之后的全部内容）。
    /// </summary>
    /// <param name="text">表达式原始文本。</param>
    /// <returns>解析结果；失败时包含出错字符在表达式内的偏移与错误描述。</returns>
    public WhereParseResult Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (WhereTokenizer.TryTokenize(text, out var tokens, out int lexOffset, out string lexError) is false)
            return WhereParseResult.Failure(lexOffset, lexError);

        if (tokens.Count is 1)
            return WhereParseResult.Failure(0, "where 表达式为空");

        _tokens = tokens;
        _position = 0;

        try
        {
            WhereExpressionNode root = ParseOr();
            if (Peek().Kind is not WhereTokenKind.EndOfInput)
                return WhereParseResult.Failure(Peek().Start, $"表达式多余的记号 '{Peek().Text}'");

            return WhereParseResult.Success(root);
        }
        catch (WhereParseException exception)
        {
            return WhereParseResult.Failure(exception.Offset, exception.Message);
        }
    }

    private WhereExpressionNode ParseOr()
    {
        int start = Peek().Start;
        WhereExpressionNode left = ParseAnd();
        while (Peek().Kind is WhereTokenKind.Or)
        {
            Advance();
            WhereExpressionNode right = ParseAnd();
            left = new WhereLogicalNode
            {
                IsAnd = false,
                Left = left,
                Right = right,
                Start = start,
                End = right.End,
            };
        }

        return left;
    }

    private WhereExpressionNode ParseAnd()
    {
        int start = Peek().Start;
        WhereExpressionNode left = ParseComparison();
        while (Peek().Kind is WhereTokenKind.And)
        {
            Advance();
            WhereExpressionNode right = ParseComparison();
            left = new WhereLogicalNode
            {
                IsAnd = true,
                Left = left,
                Right = right,
                Start = start,
                End = right.End,
            };
        }

        return left;
    }

    private WhereExpressionNode ParseComparison()
    {
        int start = Peek().Start;
        WhereExpressionNode left = ParseUnary();
        while (TryGetBinaryOperator(Peek().Kind, out WhereBinaryOperator op))
        {
            Advance();
            WhereExpressionNode right = ParseUnary();
            left = new WhereBinaryNode
            {
                Operator = op,
                Left = left,
                Right = right,
                Start = start,
                End = right.End,
            };
        }

        return left;
    }

    private WhereExpressionNode ParseUnary()
    {
        WhereToken token = Peek();
        if (token.Kind is WhereTokenKind.Not)
        {
            Advance();
            WhereExpressionNode operand = ParseUnary();
            return new WhereNotNode
            {
                Operand = operand,
                Start = token.Start,
                End = operand.End,
            };
        }

        return ParsePostfix();
    }

    private WhereExpressionNode ParsePostfix()
    {
        WhereExpressionNode target = ParseAtom();

        while (true)
        {
            WhereToken token = Peek();
            if (token.Kind is WhereTokenKind.Question)
            {
                // '?' 只在 '?.' 里出现：可选链的 '?.' 就是单个 token，带 IsConditionalAccess 标记
                // （见 WhereTokenizer）；单独的 '?' 不是合法语法。
                throw MakeParseException(token.Start, "可选链应写成 '?.'");
            }

            if (token.Kind is not WhereTokenKind.Dot)
                return target;

            // '?.' 与 '.' 都是 Dot token，可选链标记记在 dot token 上（见 WhereTokenizer.Dot）。
            bool conditional = token.IsConditionalAccess;
            Advance(); // 跳过 '.' 或 '?.'

            WhereToken name = Peek();
            if (name.Kind is not WhereTokenKind.Identifier)
                throw MakeParseException(name.Start, "'.' / '?.' 之后应为属性名或方法名");

            Advance();
            if (Peek().Kind is WhereTokenKind.LeftParen)
            {
                List<WhereExpressionNode> arguments = ParseArguments();
                target = new WhereMethodCallNode
                {
                    Target = target,
                    MethodName = name.Text,
                    Arguments = arguments,
                    IsConditionalReceiver = conditional,
                    Start = target.Start,
                    End = PreviousEnd(),
                };
                continue;
            }

            target = new WhereMemberAccessNode
            {
                IsConditional = conditional,
                Name = name.Text,
                Target = target,
                Start = target.Start,
                End = name.End,
            };
        }
    }

    private WhereExpressionNode ParseAtom()
    {
        WhereToken token = Peek();
        switch (token.Kind)
        {
            case WhereTokenKind.Number:
                Advance();
                return new WhereLiteralNode
                {
                    Kind = WhereLiteralKind.Number,
                    Text = token.Text,
                    Start = token.Start,
                    End = token.End,
                };

            case WhereTokenKind.Boolean:
                Advance();
                return new WhereLiteralNode
                {
                    Kind = WhereLiteralKind.Boolean,
                    Text = token.Text,
                    Start = token.Start,
                    End = token.End,
                };

            case WhereTokenKind.String:
                Advance();
                return new WhereLiteralNode
                {
                    Kind = WhereLiteralKind.String,
                    Text = token.Text,
                    Start = token.Start,
                    End = token.End,
                };

            case WhereTokenKind.SectionId:
                Advance();
                return new WhereSectionIdNode
                {
                    Start = token.Start,
                    End = token.End,
                };

            case WhereTokenKind.Identifier:
                return ParseIdentifierAtom();

            case WhereTokenKind.LeftParen:
            {
                Advance();
                WhereExpressionNode inner = ParseOr();
                if (Peek().Kind is not WhereTokenKind.RightParen)
                    throw MakeParseException(Peek().Start, "缺少右括号 ')'");

                Advance();
                return inner;
            }

            default:
                throw MakeParseException(
                    token.Start,
                    token.Kind is WhereTokenKind.EndOfInput ? "where 表达式意外结束" : $"意外的记号 '{token.Text}'");
        }
    }

    private WhereExpressionNode ParseIdentifierAtom()
    {
        WhereToken token = Peek();
        Advance();

        // is <TypeName>（TODO.md §4.3.4）
        if (IsKeyword(token.Text, "is"))
        {
            WhereToken typeName = Peek();
            if (typeName.Kind is not WhereTokenKind.Identifier)
            {
                throw MakeParseException(
                    typeName.Start,
                    "'is' 之后应为类型名（Entity / Registry / Global / EnumSection / Unknown）");
            }

            Advance();
            return new WhereIsNode
            {
                TypeName = typeName.Text,
                Start = token.Start,
                End = typeName.End,
            };
        }

        // 函数调用 exists(xxx)（TODO.md §4.3.1）
        if (Peek().Kind is WhereTokenKind.LeftParen)
        {
            IReadOnlyList<WhereExpressionNode> arguments = ParseArguments();
            return new WhereFunctionCallNode
            {
                FunctionName = token.Text,
                Arguments = arguments,
                Start = token.Start,
                End = PreviousEnd(),
            };
        }

        return new WherePropertyNode
        {
            Name = token.Text,
            Start = token.Start,
            End = token.End,
        };
    }

    private List<WhereExpressionNode> ParseArguments()
    {
        Advance(); // 跳过 '('
        List<WhereExpressionNode> arguments = [];
        if (Peek().Kind is WhereTokenKind.RightParen)
        {
            Advance();
            return arguments;
        }

        while (true)
        {
            arguments.Add(ParseOr());

            WhereToken separator = Peek();
            if (separator.Kind is WhereTokenKind.Comma)
            {
                Advance();
                continue;
            }

            if (separator.Kind is WhereTokenKind.RightParen)
            {
                Advance();
                return arguments;
            }

            throw MakeParseException(separator.Start, "参数之间缺少 ',' 或缺少右括号 ')'");
        }
    }

    private WhereToken Peek()
        => _tokens[_position];

    private void Advance()
        => _position++;

    private int PreviousEnd()
        => _tokens[_position - 1].End;

    private static bool TryGetBinaryOperator(WhereTokenKind kind, out WhereBinaryOperator result)
    {
        switch (kind)
        {
            case WhereTokenKind.GreaterThan:
                result = WhereBinaryOperator.GreaterThan;
                return true;
            case WhereTokenKind.GreaterOrEqual:
                result = WhereBinaryOperator.GreaterOrEqual;
                return true;
            case WhereTokenKind.LessThan:
                result = WhereBinaryOperator.LessThan;
                return true;
            case WhereTokenKind.LessOrEqual:
                result = WhereBinaryOperator.LessOrEqual;
                return true;
            case WhereTokenKind.Equal:
                result = WhereBinaryOperator.Equal;
                return true;
            case WhereTokenKind.NotEqual:
                result = WhereBinaryOperator.NotEqual;
                return true;
            default:
                result = default;
                return false;
        }
    }

    private static bool IsKeyword(string text, string keyword)
        => string.Equals(text, keyword, StringComparison.OrdinalIgnoreCase);

    private static WhereParseException MakeParseException(int offset, string message)
        => new(offset, message);
}

/// <summary>
/// <c>where</c> 表达式解析结果。
/// </summary>
/// <param name="Succeeded">是否解析成功。</param>
/// <param name="Root">成功时的语法树根；失败时为 <see langword="null"/>。</param>
/// <param name="ErrorOffset">失败时出错字符在表达式内的偏移；成功时为 -1。</param>
/// <param name="ErrorMessage">失败时的错误描述；成功时为空串。</param>
public sealed record class WhereParseResult(
    bool Succeeded,
    WhereExpressionNode? Root,
    int ErrorOffset,
    string ErrorMessage)
{
    /// <summary>
    /// 构造成功结果。
    /// </summary>
    public static WhereParseResult Success(WhereExpressionNode root)
        => new(true, root, -1, string.Empty);

    /// <summary>
    /// 构造失败结果。
    /// </summary>
    public static WhereParseResult Failure(int errorOffset, string errorMessage)
        => new(false, null, errorOffset, errorMessage);
}

/// <summary>
/// 递归下降过程中的内部异常，携带出错偏移；在 <see cref="WhereParser.Parse"/> 边界转成结果对象。
/// </summary>
internal sealed class WhereParseException(int offset, string message) : Exception(message)
{
    public int Offset { get; } = offset;
}

