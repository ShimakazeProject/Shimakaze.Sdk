namespace Shimakaze.Sdk.Ra2lyn.Common.Template;

/// <summary>
/// <c>where</c> 表达式的语法树（TODO.md §4.3）。
/// </summary>
/// <remarks>
/// 表达式在模板节里按原始字符串保存（<see cref="Common.Ir.EntityModel.TemplateWhere"/>），
/// 由 <see cref="WhereExpressionEvaluator"/> 解析后求值。语法树节点携带在原始表达式内的偏移，
/// 供诊断定位。
/// <para/>
/// 本文件只描述"文本 → 语法树"的结果结构，不依赖 <c>Common</c> 的 IR 类型：求值期的属性解析
/// 由求值器通过回调完成。
/// </remarks>
public abstract class WhereExpressionNode
{
    /// <summary>
    /// 表达式内的起始偏移。
    /// </summary>
    public int Start { get; init; }

    /// <summary>
    /// 表达式内的结束偏移（不含）。
    /// </summary>
    public int End { get; init; }
}

/// <summary>
/// 字面量：数字或字符串（TODO.md §4.3.2 右操作数）。
/// </summary>
/// <remarks>
/// 字面量始终是"有值"的，不可能是 null。
/// </remarks>
public sealed class WhereLiteralNode : WhereExpressionNode
{
    /// <summary>
    /// 字面量种类。
    /// </summary>
    public required WhereLiteralKind Kind { get; init; }

    /// <summary>
    /// 字面量的文本；字符串字面量已去掉引号、已处理转义。
    /// </summary>
    public required string Text { get; init; }
}

/// <summary>
/// <see cref="WhereLiteralNode"/> 的种类。
/// </summary>
public enum WhereLiteralKind
{
    /// <summary>
    /// 数字字面量，例如 <c>0</c>、<c>-1</c>、<c>3.5</c>。
    /// </summary>
    Number,

    /// <summary>
    /// 字符串字面量，例如 <c>"INF"</c>。
    /// </summary>
    String,

    /// <summary>
    /// 布尔字面量 <c>true</c> / <c>false</c>（TODO.md §4.3.5.3 的示例用到 <c>!= true</c>）。
    /// </summary>
    Boolean,
}

/// <summary>
/// 属性引用（TODO.md §4.3.2）：<c>Techlevel</c>、<c>PRI</c>，以及节名伪属性 <c>$id</c>（§4.3.3）。
/// </summary>
public sealed class WherePropertyNode : WhereExpressionNode
{
    /// <summary>
    /// 属性名，保留源文本大小写；比较时不敏感。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 是否为节名伪属性 <c>$id</c>（<c>$ID</c> 等写法同样成立）。
    /// </summary>
    public bool IsSectionId { get; init; }
}

/// <summary>
/// 类型匹配 <c>is &lt;TypeName&gt;</c>（TODO.md §4.3.4）。
/// </summary>
public sealed class WhereIsNode : WhereExpressionNode
{
    /// <summary>
    /// 类型名原文，例如 <c>Entity</c>、<c>Registry</c>。
    /// </summary>
    public required string TypeName { get; init; }
}

/// <summary>
/// 关系比较：<c>&gt;</c> <c>&lt;</c> <c>&gt;=</c> <c>&lt;=</c> <c>==</c> <c>!=</c>。
/// </summary>
public sealed class WhereBinaryNode : WhereExpressionNode
{
    /// <summary>
    /// 运算符。
    /// </summary>
    public required WhereBinaryOperator Operator { get; init; }

    /// <summary>
    /// 左操作数。
    /// </summary>
    public required WhereExpressionNode Left { get; init; }

    /// <summary>
    /// 右操作数。
    /// </summary>
    public required WhereExpressionNode Right { get; init; }
}

/// <summary>
/// 逻辑与 / 逻辑或：<c>&amp;&amp;</c> / <c>||</c>（TODO.md §4.3.1）。
/// </summary>
public sealed class WhereLogicalNode : WhereExpressionNode
{
    /// <summary>
    /// <see langword="true"/> 表示 <c>&amp;&amp;</c>，<see langword="false"/> 表示 <c>||</c>。
    /// </summary>
    public required bool IsAnd { get; init; }

    /// <summary>
    /// 左操作数。
    /// </summary>
    public required WhereExpressionNode Left { get; init; }

    /// <summary>
    /// 右操作数。
    /// </summary>
    public required WhereExpressionNode Right { get; init; }
}

/// <summary>
/// 一元逻辑非 <c>!x</c>（TODO.md §4.3.1、§4.3.5.3）。
/// </summary>
public sealed class WhereNotNode : WhereExpressionNode
{
    /// <summary>
    /// 取反的操作数。
    /// </summary>
    public required WhereExpressionNode Operand { get; init; }
}

/// <summary>
/// 链式访问的一环：可选链 <c>?.</c> 或普通链 <c>.</c>。
/// </summary>
public sealed class WhereMemberAccessNode : WhereExpressionNode
{
    /// <summary>
    /// 是否使用可选链 <c>?.</c>（TODO.md §4.3.5.3）。
    /// </summary>
    /// <remarks>
    /// <see langword="true"/> 时左边为 null 则整条链短路为 null；<see langword="false"/> 时
    /// 左边为 null 触发 NullReferenceError（TODO.md §4.3.5.4）。
    /// </remarks>
    public bool IsConditional { get; init; }

    /// <summary>
    /// 被访问的属性名。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 链的接收者。
    /// </summary>
    public required WhereExpressionNode Target { get; init; }
}

/// <summary>
/// 方法调用：<c>xxx.contains(y)</c>、<c>xxx.startsWith(y)</c>、<c>xxx.endsWith(y)</c>、<c>xxx.trim()</c>。
/// </summary>
public sealed class WhereMethodCallNode : WhereExpressionNode
{
    /// <summary>
    /// 方法名原文。
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// 实参列表；<c>trim()</c> 为空列表。
    /// </summary>
    public required IReadOnlyList<WhereExpressionNode> Arguments { get; init; }

    /// <summary>
    /// 链的接收者。
    /// </summary>
    public required WhereExpressionNode Target { get; init; }

    /// <summary>
    /// 本环是否用可选链 <c>?.</c> 接入接收者：为 <see langword="true"/> 时接收者为 null 则整条链短路为 null。
    /// </summary>
    public bool IsConditionalReceiver { get; init; }
}

/// <summary>
/// 函数调用：<c>exists(xxx)</c>（TODO.md §4.3.1）。
/// </summary>
public sealed class WhereFunctionCallNode : WhereExpressionNode
{
    /// <summary>
    /// 函数名原文。
    /// </summary>
    public required string FunctionName { get; init; }

    /// <summary>
    /// 实参列表。
    /// </summary>
    public required IReadOnlyList<WhereExpressionNode> Arguments { get; init; }
}

/// <summary>
/// 关系比较运算符。
/// </summary>
public enum WhereBinaryOperator
{
    /// <summary><c>&gt;</c></summary>
    GreaterThan,

    /// <summary><c>&gt;=</c></summary>
    GreaterOrEqual,

    /// <summary><c>&lt;</c></summary>
    LessThan,

    /// <summary><c>&lt;=</c></summary>
    LessOrEqual,

    /// <summary><c>==</c></summary>
    Equal,

    /// <summary><c>!=</c></summary>
    NotEqual,
}
