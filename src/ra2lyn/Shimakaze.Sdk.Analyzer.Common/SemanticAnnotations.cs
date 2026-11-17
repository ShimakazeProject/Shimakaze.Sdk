using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Analyzer.Common;

/// <summary>
/// 实体语义：分析层对“节”的分类与类型判断。
/// </summary>
public enum EntitySemanticKind
{
    /// <summary>
    /// 尚未被任何规则识别。
    /// </summary>
    Unknown,

    /// <summary>
    /// 登记表：其属性值登记了某个类型的成员名。
    /// </summary>
    Registry,

    /// <summary>
    /// 全局节：由系统固定引用，始终可达。
    /// </summary>
    Global,

    /// <summary>
    /// 枚举节：其属性键是枚举成员。
    /// </summary>
    EnumSection,

    /// <summary>
    /// 普通实体：遵循某个节定义。
    /// </summary>
    Entity,
}

/// <summary>
/// 语义注解：分析层附加在实体上的分类与类型信息，不写入内核模型。
/// </summary>
/// <remarks>
/// 注解按实体标识索引，可以在 Pass 之间共享与追加：一个实体可以同时拥有多个类型名
/// （登记表匹配得到的类型与发现规则追加的类型）。
/// </remarks>
public sealed class SemanticAnnotations
{
    private readonly Dictionary<EntityId, EntitySemanticKind> _kinds = [];
    private readonly Dictionary<EntityId, List<string>> _types = [];
    private readonly HashSet<EntityId> _reachable = [];

    /// <summary>
    /// 读取实体的语义分类；没有登记时返回 <see cref="EntitySemanticKind.Unknown"/>。
    /// </summary>
    public EntitySemanticKind GetKind(EntityId entity)
        => _kinds.TryGetValue(entity, out var kind) ? kind : EntitySemanticKind.Unknown;

    /// <summary>
    /// 设置实体的语义分类。
    /// </summary>
    public void SetKind(EntityId entity, EntitySemanticKind kind)
        => _kinds[entity] = kind;

    /// <summary>
    /// 读取实体拥有的全部类型名，顺序即登记顺序。
    /// </summary>
    public IReadOnlyList<string> GetTypes(EntityId entity)
        => _types.TryGetValue(entity, out var types) ? types : [];

    /// <summary>
    /// 追加一个类型名；重复类型会被忽略。
    /// </summary>
    public void AddType(EntityId entity, string typeName)
    {
        if (!_types.TryGetValue(entity, out var types))
        {
            types = [];
            _types[entity] = types;
        }

        if (types.Contains(typeName, StringComparer.OrdinalIgnoreCase) is false)
            types.Add(typeName);
    }

    /// <summary>
    /// 实体是否可达。
    /// </summary>
    public bool IsReachable(EntityId entity)
        => _reachable.Contains(entity);

    /// <summary>
    /// 标记实体可达或不可达。
    /// </summary>
    public void SetReachable(EntityId entity, bool reachable)
    {
        if (reachable)
        {
            _reachable.Add(entity);
            return;
        }

        _reachable.Remove(entity);
    }
}
