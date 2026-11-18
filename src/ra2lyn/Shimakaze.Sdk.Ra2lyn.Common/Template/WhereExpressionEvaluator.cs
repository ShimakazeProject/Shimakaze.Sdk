using System.Globalization;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Common.Template;

/// <summary>
/// <c>where</c> 表达式求值器（TODO.md §4.3）。
/// </summary>
/// <remarks>
/// <para>
/// 输入是"模板节写下的原始表达式"与"被评估的目标节"，输出是"该节是否匹配"。模板在每个目标节上
/// 各求值一次，因此表达式里引用的属性按"目标节"解析（TODO.md §4.3.2）：左操作数是属性 / <c>$id</c> /
/// <c>is</c>，右操作数可以是字面量，也可以是同一个节里的另一个属性。
/// </para>
/// <para>
/// 求值期诊断（NullReferenceError、语法错误）通过 <see cref="WhereEvaluationResult.Diagnostics"/>
/// 返回，而不是抛异常——同一个模板会评估很多节，诊断需要被收集成一条条带位置的记录。
/// </para>
/// <para>
/// 值模型：属性值一律按字符串处理（TODO.md §2.3），表达式树求值得到 <c>string?</c>；
/// <see langword="null"/> 表示"属性不存在"。比较语义见 §4.3.5.3，布尔判定见 §3.5.7.1。
/// </para>
/// <para>
/// <c>is &lt;Type&gt;</c> 目前固定为 <see langword="false"/>：基础分析器
/// （<c>ClassificationAnalyzer</c>）尚未前置到模板展开之前（TODO.md §5 的执行顺序调整未落地），
/// 类型信息还不存在。钩子见 <see cref="TypeMatcher"/>。
/// </para>
/// </remarks>
public sealed class WhereExpressionEvaluator
{
    /// <summary>
    /// 语法错误诊断码（<see cref="DiagnosticCodes.TemplateWhereSyntax"/>）。
    /// </summary>
    public const string SyntaxErrorCode = DiagnosticCodes.TemplateWhereSyntax;

    /// <summary>
    /// NullReferenceError 诊断码（<see cref="DiagnosticCodes.TemplateWhereNullReference"/>）。
    /// </summary>
    public const string NullReferenceCode = DiagnosticCodes.TemplateWhereNullReference;

    private readonly WhereParser _parser = new();
    private List<WhereDiagnostic> _diagnostics = [];
    private bool _failed;
    private EntityModel _target = null!;
    private SymbolTable _symbolTable = null!;

    /// <summary>
    /// 类型匹配钩子；为 <see langword="null"/> 时 <c>is &lt;Type&gt;</c> 恒为 <see langword="false"/>。
    /// </summary>
    /// <remarks>
    /// 基础分析器前置（TODO.md §5）完成后，由 <c>TemplateExpansionPass</c> 传入真正的类型判定。
    /// 形如"属性值为 null 则短路为 null"的语义与它无关：钩子只回答"这个节是不是该类型"。
    /// </remarks>
    public Func<EntityModel, string, bool>? TypeMatcher { get; init; }

    /// <summary>
    /// 对目标节求值一条 <c>where</c> 表达式。
    /// </summary>
    /// <param name="expression">模板节的 <c>where</c> 表达式原始文本。</param>
    /// <param name="target">被评估的目标节。</param>
    /// <param name="symbolTable">符号表，用于属性存在性检查与历史值回溯。</param>
    /// <returns>求值结果；表达式语法错误或求值期出错时携带诊断。</returns>
    public WhereEvaluationResult Evaluate(string expression, EntityModel target, SymbolTable symbolTable)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(symbolTable);

        _diagnostics = [];
        _failed = false;

        WhereParseResult parse = _parser.Parse(expression);
        if (parse.Succeeded is false)
        {
            _diagnostics.Add(new WhereDiagnostic(
                WhereDiagnosticKind.SyntaxError,
                SyntaxErrorCode,
                $"无法解析 where 表达式: {parse.ErrorMessage}",
                Math.Max(0, parse.ErrorOffset),
                1));
            return WhereEvaluationResult.Failure(false, _diagnostics);
        }

        _target = target;
        _symbolTable = symbolTable;

        try
        {
            string? value = EvaluateNode(parse.Root!);
            if (_failed)
                return WhereEvaluationResult.Failure(false, _diagnostics);

            return WhereEvaluationResult.MatchesResult(ToBoolean(value), _diagnostics);
        }
        // 求值器内部一律把"可预期的失败"转成 _failed + 诊断；这里的 catch 只兜底，
        // 避免一条畸形表达式让整个模板展开 Pass 崩溃。
        catch (InvalidOperationException exception)
        {
            _diagnostics.Add(new WhereDiagnostic(
                WhereDiagnosticKind.SyntaxError,
                SyntaxErrorCode,
                $"where 表达式求值失败: {exception.Message}",
                0,
                Math.Max(1, expression.Length)));
            return WhereEvaluationResult.Failure(false, _diagnostics);
        }
    }

    /// <summary>
    /// INI 风格布尔判定（TODO.md §3.5.7.1）：null 与空串为 false；
    /// 以 f / F / n / N / 0 开头为 false，其余非空为 true。
    /// </summary>
    public static bool ToBoolean(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value[0] is not ('f' or 'F' or 'n' or 'N' or '0');
    }

    private string? EvaluateNode(WhereExpressionNode node, bool strict = true)
    {
        switch (node)
        {
            case WhereLiteralNode { Kind: WhereLiteralKind.Boolean } boolean:
                return boolean.Text.ToLowerInvariant();

            case WhereLiteralNode literal:
                return literal.Text;

            case WherePropertyNode { IsSectionId: true }:
                return _target.Name;

            case WherePropertyNode property:
                return ResolveProperty(property, strict);

            case WhereIsNode isNode:
                return EvaluateIs(isNode) ? "true" : "false";

            case WhereMemberAccessNode member:
                return EvaluateMemberAccess(member);

            case WhereMethodCallNode method:
                return EvaluateMethodCall(method);

            case WhereFunctionCallNode function:
                return EvaluateFunctionCall(function);

            case WhereNotNode not:
            {
                string? operand = EvaluateNode(not.Operand);
                return _failed ? null : ToBoolean(operand) ? "false" : "true";
            }

            case WhereBinaryNode binary:
                return EvaluateComparison(binary);

            case WhereLogicalNode logical:
                return EvaluateLogical(logical);

            default:
                ReportDiagnostic(
                    WhereDiagnosticKind.SyntaxError,
                    SyntaxErrorCode,
                    $"无法识别的 where 表达式节点 {node.GetType().Name}",
                    node);
                return null;
        }
    }

    private string? ResolveProperty(WherePropertyNode property, bool strict = true)
    {
        // 严格模式（TODO.md §4.3.5.1）：属性不存在 + 直接引用即 NullReferenceError。
        // 作为可选链接收者时不在这里报错：交由 ShouldShortCircuit 判定（?.' 短路 / '.' 报错，
        // 见 TODO.md §4.3.5.3 / §4.3.5.4）。
        string?[] values = LookupProperty(property.Name);
        if (values.Length is 0)
        {
            if (strict)
            {
                ReportDiagnostic(
                    WhereDiagnosticKind.NullReference,
                    NullReferenceCode,
                    $"节 '{_target.Name}' 上不存在属性 '{property.Name}'，where 表达式引用了它",
                    property);
            }

            return null;
        }

        return values[0];
    }

    /// <summary>
    /// 查询目标节上的属性值；返回值按"优先级从高到低"排列，索引 0 即当前最终值。
    /// </summary>
    /// <remarks>
    /// 优先从符号表取（它保留了被覆盖的历史值，是未被优先级的唯一真源，TODO.md §3.3）；
    /// 符号表里查不到时退回 <see cref="EntityModel.Properties"/>，使单独使用本类（不经 SymbolizerPass）也可用。
    /// <para/>
    /// 属性"存在"的判定：符号表里存在条目，或目标节属性列表里有这个 key。值被合并成 <c>$null</c> 的
    /// 情况本步骤尚未剔除（NullCleanupPass 是步骤 7），因此按"存在"处理。
    /// </remarks>
    private string?[] LookupProperty(string key)
    {
        List<PropertyEntry> entries = [.. _symbolTable.Lookup(_target.Group, _target.Name, key)];
        if (entries.Count is 0)
        {
            string? currentValue = null;
            bool found = false;
            foreach (var property in _target.Properties)
            {
                if (string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase) is false)
                    continue;

                currentValue = property.Value;
                found = true;
            }

            return found ? [currentValue] : [];
        }

        entries.Sort(static (left, right) =>
        {
            int result = right.Priority.CompareTo(left.Priority);
            return result is not 0 ? result : right.FrontendIndex.CompareTo(left.FrontendIndex);
        });

        return [.. entries.Select(static entry => (string?)entry.Value)];
    }

    /// <summary>
    /// 可选链的前置判定：是否应该因为接收者为 null 而短路（返回 <see langword="true"/> 表示短路）。
    /// </summary>
    /// <remarks>
    /// 对照表（TODO.md §4.3.5.3 / §4.3.5.4）：
    /// <list type="bullet">
    /// <item><c>?.</c> + null → 短路返回 null，不报错。</item>
    /// <item><c>.</c> + null → NullReferenceError，视为短路（结果无效）。</item>
    /// <item>接收者非 null（含空字符串）→ <b>不</b>短路，继续执行 <c>?.method()</c> / <c>.method()</c>。</item>
    /// </list>
    /// </remarks>
    private bool ShouldShortCircuit(bool isConditional, WhereExpressionNode receiver, string? value)
    {
        if (value is null)
        {
            if (isConditional)
                return true;

            // 单点 '.' 遇到 null 立即报 NullReferenceError（TODO.md §4.3.5.4）。
            ReportDiagnostic(
                WhereDiagnosticKind.NullReference,
                NullReferenceCode,
                $"where 表达式在节 '{_target.Name}' 上对 null 使用了单点 '.': 左侧属性不存在",
                receiver);
            return true;
        }

        // 空字符串不是 null（TODO.md §4.3.5.2）：不短路，正常执行后续方法调用。
        return false;
    }

    private string? EvaluateMemberAccess(WhereMemberAccessNode member)
    {
        string? receiver = EvaluateNode(member.Target, strict: member.IsConditional is false);
        if (_failed)
            return null;

        if (ShouldShortCircuit(member.IsConditional, member.Target, receiver))
            return null;

        // 属性访问只对属性值有意义；对字符串本身没有可访问的"成员"。
        ReportDiagnostic(
            WhereDiagnosticKind.SyntaxError,
            SyntaxErrorCode,
            $"where 表达式不支持对属性值访问成员 '.{member.Name}'（只支持方法调用 contains / startsWith / endsWith / trim）",
            member);
        return null;
    }

    private string? EvaluateMethodCall(WhereMethodCallNode call)
    {
        // 可选链 ?.：接收者的"属性不存在"不在这里报错，由 ShouldShortCircuit 短路为 null。
        string? receiver = EvaluateNode(call.Target, strict: call.IsConditionalReceiver is false);
        if (_failed)
            return null;

        if (ShouldShortCircuit(call.IsConditionalReceiver, call.Target, receiver))
            return null;

        switch (call.MethodName.ToUpperInvariant())
        {
            case "TRIM":
            {
                if (call.Arguments.Count is not 0)
                {
                    ReportDiagnostic(WhereDiagnosticKind.SyntaxError, SyntaxErrorCode, "trim() 不接受参数", call);
                    return null;
                }

                // 空字符串不是 null（TODO.md §4.3.5.2），因此这里正常执行并返回 ""。
                return receiver!.Trim();
            }

            case "CONTAINS":
            case "STARTSWITH":
            case "ENDSWITH":
            {
                if (call.Arguments.Count is not 1)
                {
                    ReportDiagnostic(
                        WhereDiagnosticKind.SyntaxError,
                        SyntaxErrorCode,
                        $"{call.MethodName}() 需要且只接受 1 个字符串参数",
                        call);
                    return null;
                }

                string? argument = EvaluateNode(call.Arguments[0]);
                if (_failed)
                    return null;

                if (argument is null)
                {
                    ReportDiagnostic(
                        WhereDiagnosticKind.SyntaxError,
                        SyntaxErrorCode,
                        $"{call.MethodName}() 的参数不能为 null",
                        call.Arguments[0]);
                    return null;
                }

                return call.MethodName.ToUpperInvariant() switch
                {
                    "CONTAINS" => receiver!.Contains(argument, StringComparison.Ordinal) ? "true" : "false",
                    "STARTSWITH" => receiver!.StartsWith(argument, StringComparison.Ordinal) ? "true" : "false",
                    _ => receiver!.EndsWith(argument, StringComparison.Ordinal) ? "true" : "false",
                };
            }

            default:
                ReportDiagnostic(
                    WhereDiagnosticKind.SyntaxError,
                    SyntaxErrorCode,
                    $"where 表达式不支持方法 '{call.MethodName}'（支持 contains / startsWith / endsWith / trim）",
                    call);
                return null;
        }
    }

    private string? EvaluateFunctionCall(WhereFunctionCallNode call)
    {
        if (string.Equals(call.FunctionName, "exists", StringComparison.OrdinalIgnoreCase) is false)
        {
            ReportDiagnostic(
                WhereDiagnosticKind.SyntaxError,
                SyntaxErrorCode,
                $"where 表达式不支持函数 '{call.FunctionName}'（只支持 exists(xxx)）",
                call);
            return null;
        }

        if (call.Arguments.Count is not 1)
        {
            ReportDiagnostic(WhereDiagnosticKind.SyntaxError, SyntaxErrorCode, "exists() 需要且只接受 1 个属性名", call);
            return null;
        }

        if (call.Arguments[0] is not WherePropertyNode property)
        {
            ReportDiagnostic(WhereDiagnosticKind.SyntaxError, SyntaxErrorCode, "exists() 的参数必须是属性名", call.Arguments[0]);
            return null;
        }

        // exists 是"属性存在"测试：属性不存在时返回 false，而不是触发 NullReferenceError。
        return LookupProperty(property.IsSectionId ? "$id" : property.Name).Length > 0 ? "true" : "false";
    }

    private bool EvaluateIs(WhereIsNode isNode)
    {
        // hook：基础分析器前置后由 TypeMatcher 给出真实类型；当前未接线，恒为 false。
        return TypeMatcher is not null && TypeMatcher(_target, isNode.TypeName);
    }

    private string? EvaluateLogical(WhereLogicalNode logical)
    {
        // 逻辑运算仍对两侧求值（不做短路）：这样右侧的 NullReferenceError 不会被左侧短路掩盖。
        // TODO.md §3.3 对 #if 的处理是"先完整解析再求值"，这里保持一致。
        string? left = EvaluateNode(logical.Left);
        bool failedLeft = _failed;
        string? right = EvaluateNode(logical.Right);
        if (failedLeft || _failed)
            return null;

        bool leftValue = ToBoolean(left);
        bool rightValue = ToBoolean(right);
        return logical.IsAnd
            ? (leftValue && rightValue) ? "true" : "false"
            : (leftValue || rightValue) ? "true" : "false";
    }

    private string? EvaluateComparison(WhereBinaryNode binary)
    {
        string? left = EvaluateNode(binary.Left);
        bool failedLeft = _failed;
        string? right = EvaluateNode(binary.Right);
        if (failedLeft || _failed)
            return null;

        return Compare(binary.Operator, left, right) ? "true" : "false";
    }

    /// <summary>
    /// 比较两个值（TODO.md §4.3.5.3）：null 只对 <c>==</c> / <c>!=</c> 有特殊语义，其余比较只要有 null 即 false。
    /// </summary>
    /// <remarks>
    /// 两侧都有值时的比较规则：
    /// <list type="bullet">
    /// <item>两侧都能解析为数字 → 按数字比较（<c>Techlevel &gt;= 0</c>）。</item>
    /// <item>否则按字符串比较：<c>==</c> / <c>!=</c> 大小写不敏感（INI key / 节名语义），大小比较序数比较。</item>
    /// </list>
    /// </remarks>
    private static bool Compare(WhereBinaryOperator op, string? left, string? right)
    {
        if (left is null || right is null)
        {
            return op switch
            {
                WhereBinaryOperator.Equal => left is null && right is null,
                WhereBinaryOperator.NotEqual => (left is null) != (right is null),
                _ => false,
            };
        }

        if (TryCompareAsNumbers(left, right, out int numeric))
        {
            return op switch
            {
                WhereBinaryOperator.GreaterThan => numeric > 0,
                WhereBinaryOperator.GreaterOrEqual => numeric >= 0,
                WhereBinaryOperator.LessThan => numeric < 0,
                WhereBinaryOperator.LessOrEqual => numeric <= 0,
                WhereBinaryOperator.Equal => numeric is 0,
                _ => numeric is not 0,
            };
        }

        // 相等 / 不等：大小写不敏感（与 INI 的 key / 节名语义一致）。
        bool isEqualOperator = op is WhereBinaryOperator.Equal or WhereBinaryOperator.NotEqual;
        if (isEqualOperator)
        {
            bool same = string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
            return op is WhereBinaryOperator.Equal ? same : same is false;
        }

        // 大小比较：按序数比较。
        int order = string.CompareOrdinal(left, right);
        return op switch
        {
            WhereBinaryOperator.GreaterThan => order > 0,
            WhereBinaryOperator.GreaterOrEqual => order >= 0,
            WhereBinaryOperator.LessThan => order < 0,
            _ => order <= 0,
        };
    }

    private static bool TryCompareAsNumbers(string left, string right, out int result)
    {
        result = 0;
        if (TryParseNumber(left, out double leftValue) && TryParseNumber(right, out double rightValue))
        {
            result = leftValue.CompareTo(rightValue);
            return true;
        }

        return false;
    }

    private static bool TryParseNumber(string text, out double value)
        => double.TryParse(
            text.Trim(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out value);

    private void ReportDiagnostic(WhereDiagnosticKind kind, string code, string message, WhereExpressionNode node)
    {
        _failed = true;
        _diagnostics.Add(new WhereDiagnostic(kind, code, message, node.Start, Math.Max(1, node.End - node.Start)));
    }
}

/// <summary>
/// <c>where</c> 求值产生的诊断种类。
/// </summary>
public enum WhereDiagnosticKind
{
    /// <summary>
    /// 表达式语法错误（无法解析、未知方法等）。
    /// </summary>
    SyntaxError,

    /// <summary>
    /// NullReferenceError（TODO.md §4.3.5.4）。
    /// </summary>
    NullReference,
}

/// <summary>
/// 一条 <c>where</c> 求值诊断，偏移相对表达式文本，由调用方换算成源文件里的 <c>SourceSpan</c>。
/// </summary>
/// <param name="Kind">诊断种类。</param>
/// <param name="Code">诊断代码。</param>
/// <param name="Message">面向用户的消息。</param>
/// <param name="Offset">出错点在表达式文本内的起始偏移。</param>
/// <param name="Length">出错片段的长度。</param>
public sealed record class WhereDiagnostic(
    WhereDiagnosticKind Kind,
    string Code,
    string Message,
    int Offset,
    int Length);

/// <summary>
/// <c>where</c> 表达式求值结果。
/// </summary>
/// <param name="Succeeded">是否求值成功；为 <see langword="false"/> 时 <see cref="Matches"/> 恒为 <see langword="false"/>。</param>
/// <param name="Matches">目标节是否匹配模板条件。</param>
/// <param name="Diagnostics">求值期诊断。</param>
public sealed record class WhereEvaluationResult(
    bool Succeeded,
    bool Matches,
    IReadOnlyList<WhereDiagnostic> Diagnostics)
{
    /// <summary>
    /// 构造成功结果。
    /// </summary>
    public static WhereEvaluationResult MatchesResult(bool matches, IReadOnlyList<WhereDiagnostic> diagnostics)
        => new(true, matches, diagnostics);

    /// <summary>
    /// 构造失败结果。
    /// </summary>
    public static WhereEvaluationResult Failure(bool matches, IReadOnlyList<WhereDiagnostic> diagnostics)
        => new(false, matches, diagnostics);
}
