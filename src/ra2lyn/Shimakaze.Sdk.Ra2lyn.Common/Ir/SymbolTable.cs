using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Ir;

/// <summary>
/// 符号表中一条属性登记记录（TODO.md §3.3、§5.1）。
/// </summary>
/// <param name="Value">原始值字符串，不做类型转换。</param>
/// <param name="Priority">该属性所属 entity 的覆盖优先级（源自 <c>#pragma priority &lt;n&gt;</c>）。</param>
/// <param name="FrontendIndex">该属性所属前端在编译上下文 <see cref="Compilation.CompilationContext.Frontends"/> 中的序号。</param>
/// <param name="Origin">属性在源文件中的位置。</param>
/// <param name="IsAppend">是否由追加（<c>$</c>）合并产生；当前步骤固定为 <see langword="false"/>。</param>
public sealed record PropertyEntry(
    string Value,
    int Priority,
    int FrontendIndex,
    SourceSpan Origin,
    bool IsAppend = false);

/// <summary>
/// 符号表：按 <c>(Group, Name, Key)</c> 三维键登记所有原始属性（TODO.md §3.3、§5.1）。
/// </summary>
/// <remarks>
/// 同一个 <c>(Group, Name, Key)</c> 的多个 entry 按出现顺序追加，保留全部历史值
/// （包括被高 priority 覆盖掉的旧值），供后续 MergePass / ValueResolutionPass / 模板 where
/// 按优先级与时间戳回溯。
/// <para/>
/// 比较语义：Group / Name / Key 三个分量均使用 <see cref="StringComparer.OrdinalIgnoreCase"/>。
/// 实现上把三元组拼成单字符串 <c>"Group\0Name\0Key"</c> 作为外层字典的 key，由字典自带的
/// <see cref="StringComparer.OrdinalIgnoreCase"/> 统一处理大小写不敏感比较，避免手写
/// <see cref="IEqualityComparer{T}"/>。
/// </remarks>
public sealed class SymbolTable
{
    private readonly Dictionary<string, List<PropertyEntry>> _entries
        = new(StringComparer.OrdinalIgnoreCase);

    // 节枚举使用 "Group\0Name" 作为复合 key，与 Group / Name 均 OrdinalIgnoreCase 的语义一致。
    private readonly HashSet<string> _sectionKeys = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<(string Group, string Name)> _sectionOrder = [];

    /// <summary>
    /// 登记一条属性。同一 <c>(Group, Name, Key)</c> 的多条记录按调用顺序追加。
    /// </summary>
    public void Add(string group, string name, string key, PropertyEntry entry)
    {
        string composite = MakeKey(group, name, key);
        if (_entries.TryGetValue(composite, out var list) is false)
        {
            list = [];
            _entries[composite] = list;
        }

        list.Add(entry);

        string sectionKey = string.Concat(group, "\0", name);
        if (_sectionKeys.Add(sectionKey))
            _sectionOrder.Add((group, name));
    }

    /// <summary>
    /// 查询某个 <c>(Group, Name, Key)</c> 名下的全部 entry；不存在时返回空列表。
    /// </summary>
    public IReadOnlyList<PropertyEntry> Lookup(string group, string name, string key)
        => _entries.TryGetValue(MakeKey(group, name, key), out var list)
            ? list
            : [];

    /// <summary>
    /// 枚举所有出现过的 <c>(Group, Name)</c> 节，按首次登记顺序返回，已去重。
    /// </summary>
    public IEnumerable<(string Group, string Name)> Sections => _sectionOrder;

    private static string MakeKey(string group, string name, string key)
        => string.Concat(group, "\0", name, "\0", key);
}
