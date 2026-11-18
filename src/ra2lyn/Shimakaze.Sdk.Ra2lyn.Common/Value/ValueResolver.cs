using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Common.Value;

/// <summary>
/// 变量求值的诊断种类。
/// </summary>
public enum VariableDiagnosticKind
{
    /// <summary>
    /// <c>$</c> 引用在词法层就无法识别（未配对的 <c>${</c>、<c>${}</c>、<c>::</c> 之后又出现 <c>.</c> 等）。
    /// </summary>
    Syntax,

    /// <summary>
    /// <c>$</c> 引用解析不到任何属性（<c>::</c> / <c>.</c> 链不匹配，TODO.md §3.5.6）。
    /// </summary>
    Unresolved,

    /// <summary>
    /// 属性之间循环引用（TODO.md §3.4）。
    /// </summary>
    Circular,

    /// <summary>
    /// 递归深度超过 <see cref="CompilationOptions.MaxRecursionDepth"/>（TODO.md §3.4）。
    /// </summary>
    RecursionLimit,
}

/// <summary>
/// 一个属性在符号空间里的三维地址 <c>(Group, Name, Key)</c>（TODO.md §3.3）。
/// </summary>
/// <param name="Group">规则组。</param>
/// <param name="Name">节名。</param>
/// <param name="Key">键名。</param>
public readonly record struct PropertyAddress(string Group, string Name, string Key)
{
    /// <summary>
    /// 用作字典键的复合字符串；比较一律大小写不敏感。
    /// </summary>
    public string Composite
        => string.Concat(Group, "\0", Name, "\0", Key);

    /// <summary>
    /// 供诊断使用的可读写法 <c>Group::Name.Key</c>。
    /// </summary>
    public override string ToString()
        => string.Concat(Group, "::", Name, ".", Key);
}

/// <summary>
/// 一条变量求值诊断；偏移相对属性值文本，由调用方换算成源文件里的 <c>SourceSpan</c>。
/// </summary>
/// <param name="Kind">诊断种类。</param>
/// <param name="Code">诊断代码。</param>
/// <param name="Message">面向用户的消息。</param>
/// <param name="Offset">出错点在属性值文本内的起始偏移。</param>
/// <param name="Length">出错片段的长度。</param>
/// <param name="Owner">出错的属性。</param>
public sealed record class VariableDiagnostic(
    VariableDiagnosticKind Kind,
    string Code,
    string Message,
    int Offset,
    int Length,
    PropertyAddress Owner);

/// <summary>
/// 一次变量替换的结果。
/// </summary>
/// <param name="Values">每个求值成功的属性的最终值，键为 <see cref="PropertyAddress.Composite"/>。</param>
/// <param name="Diagnostics">求值诊断。</param>
public sealed record class ResolutionResult(
    IReadOnlyDictionary<string, ResolvedValue> Values,
    IReadOnlyList<VariableDiagnostic> Diagnostics);

/// <summary>
/// 属性级变量替换器：把所有待求值属性组织成依赖图，按依赖顺序递归展开
/// （TODO.md §3.4、§3.5）。
/// </summary>
/// <remarks>
/// <para>
/// 输入是<b>已合并、已应用模板</b>的实体列表；输出是每个 <c>(Group, Name, Key)</c> 的最终值。
/// 所有属性先按 <c>(Group, Name, Key)</c> 建节点；递归求值时先求值被引用的属性，
/// 因此一个属性只求一次，且总是按依赖顺序完成——等价于在这张依赖图上做一次拓扑排序。
/// </para>
/// <para>
/// 求值失败（引用写法无法识别、解析不到属性、循环引用、超出递归深度）时该属性保留<b>原始字符串</b>，
/// 同时产生一条 error 诊断（TODO.md §3.4 / §3.5.6）。失败的属性不会污染引用它的属性：
/// 引用方按字符串拼接时用的是它的原始字符串，而不是"半成品"。
/// </para>
/// <para>
/// 裸 <c>$</c> 占位符取"当前节当前 key 的历史值与源文本按顺序 merge 出来的当前值"，
/// 即 <see cref="PropertyModel.History"/> 链第一条的当前值（TODO.md §2.1：同节里多次
/// <c>Owner=$,X</c> 依次追加），而不是合并后的最终值本身（那会变成自我引用）。
/// 当前值"不存在"（History 为空，或链上被 <c>$null</c> 覆盖）时按空字符串拼接：
/// <c>Owner=$,D</c> 得到 <c>,D</c>（TODO.md §3.5.8）。
/// </para>
/// </remarks>
public sealed class ValueResolver
{
    /// <summary>
    /// 常量 <c>$null</c> 的文本（大小写不敏感，TODO.md §3.5.7）。
    /// </summary>
    public const string NullConstant = "null";

    private const string DoubleColonSeparator = "::";
    private const string DollarPrefix = "$";

    private readonly Dictionary<string, PropertyNode> _nodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<PropertyNode> _nodeOrder = [];
    private readonly Dictionary<string, ResolvedValue> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<VariableDiagnostic> _diagnostics = [];
    private int _maxDepth = CompilationOptions.DefaultMaxRecursionDepth;

    /// <summary>
    /// 对一批实体做变量替换，返回每个属性的最终值。
    /// </summary>
    /// <param name="entities">已合并、已应用模板的实体列表。</param>
    /// <param name="maxRecursionDepth">递归深度上限；非正数使用 <see cref="CompilationOptions.DefaultMaxRecursionDepth"/>。</param>
    /// <returns>求值结果。</returns>
    /// <remarks>
    /// 同一个 <c>(Group, Name, Key)</c> 在 <paramref name="entities"/> 里出现多次时以最后一个为准
    /// （例如 <c>#pragma template off</c> 后按普通节保留的 <c>$</c> 开头节，其后的同名节覆盖它）。
    /// </remarks>
    public ResolutionResult Resolve(IReadOnlyList<EntityModel> entities, int maxRecursionDepth)
    {
        ArgumentNullException.ThrowIfNull(entities);

        _nodes.Clear();
        _nodeOrder.Clear();
        _values.Clear();
        _diagnostics.Clear();
        _maxDepth = maxRecursionDepth > 0 ? maxRecursionDepth : CompilationOptions.DefaultMaxRecursionDepth;

        BuildGraph(entities);

        HashSet<string> stack = new(StringComparer.OrdinalIgnoreCase);
        foreach (var node in _nodeOrder)
            ResolveNode(node, depth: 0, stack);

        return new ResolutionResult(_values, [.. _diagnostics]);
    }

    #region 依赖图

    /// <summary>
    /// 为每个实体属性建节点。
    /// </summary>
    /// <remarks>
    /// 这里只做"登记"，不做引用解析：引用的目标随 <c>(Group, Name)</c> 上下文解析，
    /// 留到递归求值时按需进行，避免为同一属性重复扫描表达式。
    /// </remarks>
    private void BuildGraph(IReadOnlyList<EntityModel> entities)
    {
        foreach (var entity in entities)
        {
            // 模板节已由 TemplateExpansionPass 剔除；残留的模板节不参与求值（TODO.md §4.5）。
            if (entity.IsTemplate)
                continue;

            foreach (var property in entity.Properties)
            {
                PropertyAddress address = new(entity.Group, entity.Name, property.Key);
                string composite = address.Composite;

                // 同一个 (Group, Name, Key) 重复出现时以最后一个为准：先移除旧节点，保持顺序稳定。
                if (_nodes.Remove(composite, out var replaced))
                    _nodeOrder.Remove(replaced);

                PropertyNode node = new()
                {
                    Address = address,
                    Composite = composite,
                    Original = property.Value,
                    History = property.History,
                };

                _nodes[composite] = node;
                _nodeOrder.Add(node);
            }
        }
    }

    #endregion

    #region 求值

    /// <summary>
    /// 递归求值一个属性；结果缓存在节点上，保证只求一次。
    /// </summary>
    /// <param name="node">属性节点。</param>
    /// <param name="depth">当前递归深度（<c>0</c> 表示最外层属性）。</param>
    /// <param name="stack">当前递归栈，用于循环引用检测。</param>
    private ResolvedValue ResolveNode(PropertyNode node, int depth, HashSet<string> stack)
    {
        if (node.Result is not null)
            return node.Result;

        if (depth > _maxDepth)
        {
            Report(node, VariableDiagnosticKind.RecursionLimit, 0, 1,
                $"变量求值递归深度超过上限 {_maxDepth}，属性 '{node.Address}' 保留原始字符串");
            return ResolvedValue.Failed(node.Original);
        }

        if (stack.Contains(node.Composite))
        {
            Report(node, VariableDiagnosticKind.Circular, 0, 1,
                $"循环引用: 属性 '{node.Address}' 最终引用回自己，保留原始字符串");
            return ResolvedValue.Failed(node.Original);
        }

        stack.Add(node.Composite);
        try
        {
            ResolvedValue result = Evaluate(node, depth, stack);
            node.Result = result;
            _values[node.Composite] = result;
            return result;
        }
        finally
        {
            stack.Remove(node.Composite);
        }
    }

    /// <summary>
    /// 对一个属性做词法切分并逐段求值，最后按字符串拼接成最终值。
    /// </summary>
    private ResolvedValue Evaluate(PropertyNode node, int depth, HashSet<string> stack)
        => EvaluateText(node, node.Original, depth, stack, historyIndex: node.History.Count);

    /// <summary>
    /// 求值单个 <c>$</c> 引用。
    /// </summary>
    /// <param name="owner">写下该引用的属性（提供 Group / Name / Key 上下文）。</param>
    /// <param name="reference">词法识别出的引用。</param>
    /// <param name="depth">当前递归深度。</param>
    /// <param name="stack">递归栈。</param>
    /// <param name="historyIndex">当前文本在 History 链上的位置（见 <see cref="EvaluateText"/>）。</param>
    /// <param name="failed">引用求值失败（已报诊断），调用方应把整个属性保留为原始字符串。</param>
    /// <returns>拼接用的文本；当前值 / null 值都返回空字符串。</returns>
    private string? ResolveReference(
        PropertyNode owner,
        VariableReference reference,
        int depth,
        HashSet<string> stack,
        int historyIndex,
        out bool failed)
    {
        failed = false;

        // 裸 `$`：当前最终值占位符（TODO.md §2 / §3.5.5）。
        if (reference.IsCurrentValuePlaceholder)
            return ResolveCurrentValue(owner, depth, stack, historyIndex);

        string expression = reference.Expression!;

        // `$null` 是常量，不去符号表查同名 key（TODO.md §3.5.7）。
        if (IsNullConstant(expression))
            return string.Empty;

        if (VariableReferenceParser.IsPathExpression(expression) is false)
        {
            failed = true;
            Report(owner, VariableDiagnosticKind.Syntax, reference.Start, Math.Max(1, reference.Length),
                $"无法识别的变量路径 '{expression}'（'::' 两侧与 '.' 之后必须有名字，且 '.' 之后不能再出现 '::'，TODO.md §3.5.3）");
            return null;
        }

        if (TryResolvePath(owner, expression, out PropertyAddress? target) is false)
        {
            failed = true;
            Report(owner, VariableDiagnosticKind.Unresolved, reference.Start, Math.Max(1, reference.Length),
                $"变量引用 '{expression}' 指向的属性不存在: {DescribeMissing(owner, expression)}");
            return null;
        }

        string targetComposite = target!.Value.Composite;
        if (_nodes.TryGetValue(targetComposite, out var targetNode) is false)
        {
            failed = true;
            Report(owner, VariableDiagnosticKind.Unresolved, reference.Start, Math.Max(1, reference.Length),
                $"变量引用 '{expression}' 指向的属性尚未求值: {target}");
            return null;
        }

        ResolvedValue resolved = ResolveNode(targetNode, depth + 1, stack);
        if (resolved.Succeeded is false)
        {
            // 被引用属性自身求值失败：它保留的是原始字符串，引用方也保留自己的原始字符串，
            // 避免把"半成品"拼进结果里（TODO.md §3.4：出错处保留原始字符串）。
            failed = true;
            return null;
        }

        // 目标存在但值是 null：拼接用空字符串（TODO.md §3.5.8：与"不存在"在字符串拼接上等价）。
        return resolved.ConcatenationText;
    }

    /// <summary>
    /// 求值裸 <c>$</c> 占位符：取"当前节当前 key 的当前值"（TODO.md §2.1 / §3.5.8）。
    /// </summary>
    /// <remarks>
    /// "当前值"= 本属性在 History 链上<b>紧邻的前一层</b>的值：<see cref="PropertyModel.History"/>
    /// 按从旧到新排列，<c>History[historyIndex - 1]</c> 就是在这一层被覆盖掉的旧值。
    /// 同节里多次 <c>Owner=$,X</c> 时，链尾就是前一条的原始文本，天然满足 §2.1 的
    /// "每次取上一次追加后的当前值"。
    /// <para/>
    /// History 为空（key 首次出现）时当前值"不存在"→ 返回空字符串，保留前导分隔符
    /// （<c>Owner=$,D</c> 得到 <c>,D</c>，TODO.md §3.5.8）。上一层若是 <c>$null</c> 常量
    /// （被 <c>$null</c> 覆盖过）同样按"不存在"处理。
    /// <para/>
    /// 上一层的值可能还含变量（例如 <c>Owner=$,D</c> 的上一层是 <c>Owner=$,C</c>），
    /// 它要按它自己的上下文再求值一次——这时它里面的裸 <c>$</c> 指向再上一层
    /// （<c>History[historyIndex - 2]</c>），因此不会自我递归。
    /// </remarks>
    private string ResolveCurrentValue(PropertyNode owner, int depth, HashSet<string> stack, int historyIndex)
    {
        // historyIndex 是当前文本在链上的位置：链长 N 表示"最终值层"，其上一层是 History[N-1]。
        int previousIndex = historyIndex - 1;
        if (previousIndex < 0 || previousIndex >= owner.History.Count)
            return string.Empty;

        string raw = owner.History[previousIndex].Value;

        // 上一层是 $null 常量或空串：按"不存在"处理（TODO.md §3.5.8）。
        if (IsNullConstantValue(raw) || raw.Length is 0)
            return string.Empty;

        // 上一层本身不含 `$`：原样就是当前值。
        if (raw.IndexOf('$', StringComparison.Ordinal) < 0)
            return raw;

        // 上一层含变量：按它自己的上下文（historyIndex = previousIndex）再求值一次。
        ResolvedValue resolved = EvaluateText(owner, raw, depth + 1, stack, historyIndex: previousIndex);
        return resolved.Succeeded && resolved.HasValue ? resolved.Text! : string.Empty;
    }

    /// <summary>
    /// 对一段属性值文本做变量替换。
    /// </summary>
    /// <param name="owner">写下该文本的属性。</param>
    /// <param name="value">文本。</param>
    /// <param name="depth">当前递归深度。</param>
    /// <param name="stack">递归栈。</param>
    /// <param name="historyIndex">这段文本在 History 链上的位置：等于 <see cref="PropertyModel.History"/> 的
    /// 长度表示"最终值层"，更小的下标表示链上某一层的旧值（供裸 <c>$</c> 逐层回溯，TODO.md §2.1）。</param>
    private ResolvedValue EvaluateText(
        PropertyNode owner,
        string value,
        int depth,
        HashSet<string> stack,
        int historyIndex)
    {
        if (VariableReferenceParser.TryParse(value, out var segments, out string? error, out int errorOffset, out int errorLength) is false)
        {
            Report(owner, VariableDiagnosticKind.Syntax, errorOffset, Math.Max(1, errorLength),
                $"属性 '{owner.Address}' 的值无法解析: {error}");
            return ResolvedValue.Failed(value);
        }

        // 没有任何引用：原始值就是最终值。
        if (segments!.All(static segment => segment.Reference is null))
            return ResolvedValue.Present(value, value);

        // 整个值恰好是一个 `$null` 常量（`Key=$null`，TODO.md §3.5.7）：
        // 标记为 null，由 NullCleanupPass 删除 key（本步骤只标记，TODO.md §3.5.8）。
        if (segments!.Count is 1
            && segments[0].Reference?.Kind is VariableReferenceKind.Variable or VariableReferenceKind.BracedVariable
            && IsNullConstant(segments[0].Reference!.Expression!))
        {
            return ResolvedValue.Null(value);
        }

        List<string> pieces = [];
        foreach (var segment in segments)
        {
            if (segment.Reference is null)
            {
                pieces.Add(segment.Text!);
                continue;
            }

            string? text = ResolveReference(owner, segment.Reference, depth, stack, historyIndex, out bool failed);
            if (failed)
                return ResolvedValue.Failed(value);

            // 一律按字符串拼接，不解析集合（TODO.md §2.3 / §3.5.8）。
            pieces.Add(text ?? string.Empty);
        }

        return ResolvedValue.Present(string.Concat(pieces), value);
    }

    /// <summary>
    /// 按 TODO.md §3.5.1 的四种路径形式解析目标属性。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>Group::Section.Key</c>：跨 Group、跨 Section。</item>
    /// <item><c>Section.Key</c>：当前 Group，跨 Section。</item>
    /// <item><c>Key</c>：当前节（当前 Group + 当前 Name）。</item>
    /// <item><c>Group::Key</c>：跨 Group，引用该 Group 下与当前节同名的 Section（TODO.md §3.5.6）。</item>
    /// </list>
    /// Group 段允许多段 <c>::</c> 链：只有最后一段才是 Section，前面所有段依次拼回 Group
    /// （实际引用的键就是拼接后的 Group 名，见 <see cref="VariableReference"/> 的说明）。
    /// </remarks>
    private bool TryResolvePath(PropertyNode owner, string expression, out PropertyAddress? target)
    {
        target = null;

        string[] parts = expression.Split(DoubleColonSeparator, StringSplitOptions.None);
        if (parts.Length is 1)
        {
            string single = parts[0];
            if (single.Contains('.', StringComparison.Ordinal))
            {
                // `Section.Key`：当前 Group，跨 Section（TODO.md §3.5.1）。
                string[] inner = single.Split('.');
                target = new(owner.Address.Group, inner[0], inner[1]);
                return Exists(target.Value);
            }

            // `Key`：同 Group、同 Section、同 key。
            target = new(owner.Address.Group, owner.Address.Name, single);
            return Exists(target.Value);
        }

        if (parts.Length is 2)
        {
            if (parts[1].Contains('.', StringComparison.Ordinal))
            {
                // `Group::Section.Key`：跨 Group、跨 Section。
                string[] inner = parts[1].Split('.');
                target = new(parts[0], inner[0], inner[1]);
                return Exists(target.Value);
            }

            // `Group::Key`：该 Group 下与当前节同名的 Section（TODO.md §3.5.6）。
            target = new(parts[0], owner.Address.Name, parts[1]);
            return Exists(target.Value);
        }

        // 多段 `::`：只有最后一段可能是 Section，前面全部是 Group 路径。
        string groupPath = string.Join(DoubleColonSeparator, parts[..^1]);
        string tail = parts[^1];
        if (tail.Contains('.', StringComparison.Ordinal))
        {
            // `A::B::Section.Key`：整个 Group 段拼回一个 Group 名。
            string[] inner = tail.Split('.');
            target = new(groupPath, inner[0], inner[1]);
            return Exists(target.Value);
        }

        // `A::B::Key`：Group 名是拼接后的多段路径，Section 取当前节同名。
        target = new(groupPath, owner.Address.Name, tail);
        return Exists(target.Value);
    }

    /// <summary>
    /// 判断目标属性是否存在：符号空间里任何一节的任意 key 都算（变量作用域不加可见性限制，TODO.md §3.5.6）。
    /// </summary>
    private bool Exists(PropertyAddress address)
        => _nodes.ContainsKey(address.Composite);

    /// <summary>
    /// 判断一段文本是否是 <c>$null</c> 常量（大小写不敏感，TODO.md §3.5.7）。
    /// </summary>
    private static bool IsNullConstant(string expression)
        => string.Equals(expression, NullConstant, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 判断历史链里的一个原始值是否是 <c>$null</c> 常量（覆盖后按"不存在"处理，TODO.md §3.5.8）。
    /// </summary>
    private static bool IsNullConstantValue(string value)
        => string.Equals(value, string.Concat(DollarPrefix, NullConstant), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 描述"引用解析不到"的上下文，便于用户在诊断里定位。
    /// </summary>
    private static string DescribeMissing(PropertyNode owner, string expression)
        => $"当前上下文为 Group '{owner.Address.Group}'、Section '{owner.Address.Name}'，表达式 '{expression}'";

    /// <summary>
    /// 报告一条诊断；同一属性同一偏移的重复诊断只保留第一条。
    /// </summary>
    private void Report(PropertyNode owner, VariableDiagnosticKind kind, int offset, int length, string message)
    {
        int safeOffset = Math.Clamp(offset, 0, owner.Original.Length);
        int safeLength = Math.Max(1, Math.Min(length, owner.Original.Length - safeOffset));

        _diagnostics.Add(new VariableDiagnostic(
            kind,
            CodeOf(kind),
            message,
            safeOffset,
            owner.Original.Length is 0 ? 0 : safeLength,
            owner.Address));
    }

    private static string CodeOf(VariableDiagnosticKind kind)
        => kind switch
        {
            VariableDiagnosticKind.Syntax => DiagnosticCodes.VariableSyntax,
            VariableDiagnosticKind.Unresolved => DiagnosticCodes.UnresolvedVariable,
            VariableDiagnosticKind.Circular => DiagnosticCodes.CircularVariableReference,
            _ => DiagnosticCodes.VariableRecursionLimit,
        };

    #endregion

    /// <summary>
    /// 一个参与求值的属性。
    /// </summary>
    private sealed class PropertyNode
    {
        public required PropertyAddress Address { get; init; }

        public required string Composite { get; init; }

        /// <summary>属性在合并后的原始值字符串。</summary>
        public required string Original { get; init; }

        /// <summary>被覆盖掉的历史值链，从旧到新；<c>History[0]</c> 是最近一次覆盖前的值。</summary>
        public required IReadOnlyList<PropertyHistoryEntry> History { get; init; }

        /// <summary>求值结果缓存。</summary>
        public ResolvedValue? Result { get; set; }
    }
}

